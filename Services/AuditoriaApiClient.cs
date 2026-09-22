using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class AuditoriaApiClient
{
    private readonly HttpClient _http;

    public AuditoriaApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<AuditoriaDto>> ListarAsync(
        string? entidad = null, string? accion = null, int? usuarioId = null,
        DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        if (!string.IsNullOrWhiteSpace(entidad)) query["Entidad"] = entidad;
        if (!string.IsNullOrWhiteSpace(accion)) query["Accion"] = accion;
        if (usuarioId is not null) query["UsuarioId"] = usuarioId.Value.ToString();
        if (desde is not null) query["Desde"] = desde.Value.ToString("o");
        if (hasta is not null) query["Hasta"] = hasta.Value.ToString("o");

        var respuesta = await _http.GetAsync($"api/auditoria?{query}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<AuditoriaDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<CatalogoAuditoriaDto> ObtenerCatalogoAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/auditoria/catalogo", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<CatalogoAuditoriaDto>(cancellationToken: ct))!;
    }
}
