using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;

namespace Inventory.Web.Services.Auth;

// Scoped = vive mientras dure el circuito de Blazor Server (una pestaña del navegador).
// El JWT nunca sale al navegador: solo existe acá, del lado del servidor. Cada
// *ApiClient autenticado recibe esta instancia en su propio constructor y fija el
// Authorization: Bearer ahí mismo — no vía un DelegatingHandler de IHttpClientFactory,
// que arma su pipeline en un scope interno propio y nunca ve el AuthState real del
// circuito (bug real encontrado probando la app: todo el CRUD daba 401 en silencio).
public class AuthState : IDisposable
{
    // Bug real encontrado con la app corriendo (2026-09-18): con <Routes @rendermode="InteractiveServer">
    // (prerender activado, el default), App.razor SÍ restaura la sesión desde la cookie durante el
    // prerender — pero ese prerender corre en el scope de DI del request HTTP original. Apenas el
    // navegador termina de conectar el circuito real por SignalR, Blazor crea un scope de DI NUEVO para
    // ese circuito, con su propio AuthState recién instanciado y vacío — App.razor no vuelve a correr
    // (no es parte del subárbol interactivo), así que nada lo repuebla. Resultado: la página se ve bien
    // un instante (el HTML ya prerenderizado) y al toma el control el circuito interactivo, la sesión
    // aparece "vencida" (HasPermission da false para todo) y el usuario cae en un loop de redirects.
    // Arreglo oficial de Blazor para pasar datos del prerender al circuito interactivo:
    // PersistentComponentState — el framework serializa lo que se registre acá dentro del HTML inicial
    // y se lo entrega de vuelta al PersistentComponentState del circuito nuevo.
    private readonly PersistentComponentState _estadoPersistente;
    private readonly PersistingComponentStateSubscription _suscripcion;
    private const string ClaveToken = "auth-token";

    public AuthState(PersistentComponentState estadoPersistente)
    {
        _estadoPersistente = estadoPersistente;
        _suscripcion = _estadoPersistente.RegisterOnPersisting(PersistirAsync);

        if (_estadoPersistente.TryTakeFromJson<string>(ClaveToken, out var token) && token is not null)
            RestaurarDesdeToken(token);
    }

    private Task PersistirAsync()
    {
        if (IsAuthenticated)
            _estadoPersistente.PersistAsJson(ClaveToken, Token);
        return Task.CompletedTask;
    }

    public void Dispose() => _suscripcion.Dispose();

    public bool IsAuthenticated { get; private set; }
    public string Token { get; private set; } = "";
    public int UsuarioId { get; private set; }
    public string Email { get; private set; } = "";
    public string NombreCompleto { get; private set; } = "";
    public string RolNombre { get; private set; } = "";

    // País elegido en el login (ver Home.razor) — un login = un país, no cambia durante
    // el circuito. Viaja también como claim "pais" en el JWT, así que el backend filtra
    // solo, esto acá es para pintar la bandera/nombre en la UI sin otra llamada.
    public int PaisId { get; private set; }
    public string PaisNombre { get; private set; } = "";
    public string PaisCodigoIso { get; private set; } = "";

    private HashSet<string> Permisos { get; set; } = [];

    public event Action? OnChange;

    // Login real vive en el endpoint /login-cookie (Program.cs, no un componente Blazor —
    // necesita mandar un Set-Cookie de verdad, algo que un circuito ya interactivo no
    // puede hacer). Esto reconstruye el AuthState de ESTE circuito a partir del JWT que
    // guardó esa cookie, para que un F5 no desloguee — se llama una vez desde App.razor
    // (única parte del árbol que todavía tiene HttpContext disponible para leer la cookie).
    // No valida la firma del JWT acá: si alguien edita el valor de la cookie a mano, el
    // peor caso es que la UI muestre links de más — cualquier llamada real a la API igual
    // la rechaza (401), porque ahí sí se valida la firma contra Jwt:SecretKey.
    public bool RestaurarDesdeToken(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            if (jwt.ValidTo < DateTime.UtcNow)
                return false;

            IsAuthenticated = true;
            Token = token;
            UsuarioId = int.Parse(jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Email = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
            NombreCompleto = jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value;
            RolNombre = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;
            PaisId = int.Parse(jwt.Claims.First(c => c.Type == "pais").Value);
            PaisNombre = jwt.Claims.First(c => c.Type == "pais_nombre").Value;
            PaisCodigoIso = jwt.Claims.First(c => c.Type == "pais_codigo").Value;
            Permisos = jwt.Claims.Where(c => c.Type == "permiso").Select(c => c.Value).ToHashSet();

            OnChange?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool HasPermission(string codigo) => IsAuthenticated && Permisos.Contains(codigo);
}
