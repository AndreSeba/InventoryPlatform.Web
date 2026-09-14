using Inventory.Application.Dtos;

namespace Inventory.Web.Services.Auth;

// Scoped = vive mientras dure el circuito de Blazor Server (una pestaña del navegador).
// El JWT nunca sale al navegador: solo existe acá, del lado del servidor. Cada
// *ApiClient autenticado recibe esta instancia en su propio constructor y fija el
// Authorization: Bearer ahí mismo — no vía un DelegatingHandler de IHttpClientFactory,
// que arma su pipeline en un scope interno propio y nunca ve el AuthState real del
// circuito (bug real encontrado probando la app: todo el CRUD daba 401 en silencio).
public class AuthState
{
    public bool IsAuthenticated { get; private set; }
    public string Token { get; private set; } = "";
    public int UsuarioId { get; private set; }
    public string Email { get; private set; } = "";
    public string NombreCompleto { get; private set; } = "";
    public string RolNombre { get; private set; } = "";
    private HashSet<string> Permisos { get; set; } = [];

    public event Action? OnChange;

    public void SignIn(LoginResultDto resultado)
    {
        IsAuthenticated = true;
        Token = resultado.Token;
        UsuarioId = resultado.Usuario.Id;
        Email = resultado.Usuario.Email;
        NombreCompleto = resultado.Usuario.NombreCompleto;
        RolNombre = resultado.Usuario.RolNombre;
        Permisos = resultado.Usuario.Permisos.ToHashSet();
        OnChange?.Invoke();
    }

    public void SignOut()
    {
        IsAuthenticated = false;
        Token = "";
        UsuarioId = 0;
        Email = "";
        NombreCompleto = "";
        RolNombre = "";
        Permisos = [];
        OnChange?.Invoke();
    }

    public bool HasPermission(string codigo) => IsAuthenticated && Permisos.Contains(codigo);
}
