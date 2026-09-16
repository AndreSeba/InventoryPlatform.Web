using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class UnidadApiClient
{
    private readonly HttpClient _http;

    public UnidadApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<UnidadDto>> ListarAsync(bool incluirInactivas = false, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/unidades?incluirInactivas={incluirInactivas}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<UnidadDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<UnidadDto> CrearAsync(CrearUnidadDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/unidades", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<UnidadDto>(cancellationToken: ct))!;
    }

    public async Task<UnidadDto> ActualizarAsync(int id, ActualizarUnidadDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/unidades/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<UnidadDto>(cancellationToken: ct))!;
    }
}
