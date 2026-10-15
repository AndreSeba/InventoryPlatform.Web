using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

// Módulo de conteo independiente — nombres de almacén (Fase 6).
public class ConteoExternoAlmacenApiClient
{
    private readonly HttpClient _http;

    public ConteoExternoAlmacenApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<ConteoExternoAlmacenDto>> ListarAsync(bool incluirInactivos = false, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteo-externo/almacenes?incluirInactivos={incluirInactivos}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoExternoAlmacenDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<ConteoExternoAlmacenDto> CrearAsync(CrearConteoExternoAlmacenDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/conteo-externo/almacenes", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoAlmacenDto>(cancellationToken: ct))!;
    }

    public async Task<ConteoExternoAlmacenDto> ActualizarAsync(int id, ActualizarConteoExternoAlmacenDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/conteo-externo/almacenes/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoAlmacenDto>(cancellationToken: ct))!;
    }
}
