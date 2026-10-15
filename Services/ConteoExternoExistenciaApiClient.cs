using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

// Módulo de conteo independiente — existencias propias (Fase 2).
public class ConteoExternoExistenciaApiClient
{
    private readonly HttpClient _http;

    public ConteoExternoExistenciaApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<ConteoExternoExistenciaDto>> ListarAsync(int? conteoExternoProductoId = null, string? almacenCodigo = null, bool incluirInactivas = false, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (conteoExternoProductoId is not null) query.Add($"conteoExternoProductoId={conteoExternoProductoId}");
        if (!string.IsNullOrWhiteSpace(almacenCodigo)) query.Add($"almacenCodigo={Uri.EscapeDataString(almacenCodigo)}");
        if (incluirInactivas) query.Add("incluirInactivas=true");

        var url = "api/conteo-externo/existencias" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var respuesta = await _http.GetAsync(url, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoExternoExistenciaDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<ConteoExternoExistenciaDto> CrearAsync(CrearConteoExternoExistenciaDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/conteo-externo/existencias", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoExistenciaDto>(cancellationToken: ct))!;
    }

    public async Task<ConteoExternoExistenciaDto> ActualizarAsync(int id, ActualizarConteoExternoExistenciaDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/conteo-externo/existencias/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoExistenciaDto>(cancellationToken: ct))!;
    }
}
