using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class AreaApiClient
{
    private readonly HttpClient _http;

    public AreaApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<AreaDto>> ListarAsync(bool incluirInactivas = false, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/areas?incluirInactivas={incluirInactivas}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<AreaDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<AreaDto> CrearAsync(CrearAreaDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/areas", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<AreaDto>(cancellationToken: ct))!;
    }

    public async Task<AreaDto> ActualizarAsync(int id, ActualizarAreaDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/areas/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<AreaDto>(cancellationToken: ct))!;
    }
}
