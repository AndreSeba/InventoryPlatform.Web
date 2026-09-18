using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class AlmacenApiClient
{
    private readonly HttpClient _http;

    public AlmacenApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<AlmacenDto>> ListarAsync(bool incluirInactivos = false, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/almacenes?incluirInactivos={incluirInactivos}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<AlmacenDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<AlmacenDto> CrearAsync(CrearAlmacenDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/almacenes", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<AlmacenDto>(cancellationToken: ct))!;
    }

    public async Task<AlmacenDto> ActualizarAsync(int id, ActualizarAlmacenDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/almacenes/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<AlmacenDto>(cancellationToken: ct))!;
    }
}
