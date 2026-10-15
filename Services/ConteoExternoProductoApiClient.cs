using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

// Módulo de conteo independiente — catálogo de productos propio.
public class ConteoExternoProductoApiClient
{
    private readonly HttpClient _http;

    public ConteoExternoProductoApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<ConteoExternoProductoDto>> ListarAsync(string? texto = null, string? familia = null, bool incluirInactivos = false, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(texto)) query.Add($"texto={Uri.EscapeDataString(texto)}");
        if (!string.IsNullOrWhiteSpace(familia)) query.Add($"familia={Uri.EscapeDataString(familia)}");
        if (incluirInactivos) query.Add("incluirInactivos=true");

        var url = "api/conteo-externo/productos" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var respuesta = await _http.GetAsync(url, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoExternoProductoDto>>(cancellationToken: ct) ?? [];
    }

    // Autocompletado por código o nombre (hasta `max` coincidencias).
    public async Task<List<ConteoExternoProductoBusquedaDto>> BuscarAsync(string texto, int max = 6, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteo-externo/productos/buscar?texto={Uri.EscapeDataString(texto)}&max={max}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoExternoProductoBusquedaDto>>(cancellationToken: ct) ?? [];
    }

    // Unidad, costo, almacén habitual y lotes conocidos del producto elegido.
    public async Task<ConteoExternoSugerenciasDto> ObtenerSugerenciasAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteo-externo/productos/{id}/sugerencias", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoSugerenciasDto>(cancellationToken: ct))!;
    }

    public async Task<List<string>> ListarFamiliasAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/conteo-externo/productos/familias", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<string>>(cancellationToken: ct) ?? [];
    }

    public async Task<ConteoExternoProductoDto> CrearAsync(CrearConteoExternoProductoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/conteo-externo/productos", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoProductoDto>(cancellationToken: ct))!;
    }

    public async Task<ConteoExternoProductoDto> ActualizarAsync(int id, ActualizarConteoExternoProductoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/conteo-externo/productos/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoProductoDto>(cancellationToken: ct))!;
    }
}
