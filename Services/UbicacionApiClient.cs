using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class UbicacionApiClient
{
    private readonly HttpClient _http;

    public UbicacionApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<UbicacionDto>> ListarAsync(bool incluirInactivas = false, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/ubicaciones?incluirInactivas={incluirInactivas}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<UbicacionDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<UbicacionDto> CrearAsync(CrearUbicacionDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/ubicaciones", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<UbicacionDto>(cancellationToken: ct))!;
    }
}
