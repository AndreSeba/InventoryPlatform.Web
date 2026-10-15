using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

// Módulo de conteo independiente — catálogo de unidades propio.
public class ConteoExternoUnidadApiClient
{
    private readonly HttpClient _http;

    public ConteoExternoUnidadApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<ConteoExternoUnidadDto>> ListarAsync(bool incluirInactivas = false, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteo-externo/unidades?incluirInactivas={incluirInactivas}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoExternoUnidadDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<ConteoExternoUnidadDto> CrearAsync(CrearConteoExternoUnidadDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/conteo-externo/unidades", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoUnidadDto>(cancellationToken: ct))!;
    }

    public async Task<ConteoExternoUnidadDto> ActualizarAsync(int id, ActualizarConteoExternoUnidadDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/conteo-externo/unidades/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoUnidadDto>(cancellationToken: ct))!;
    }
}
