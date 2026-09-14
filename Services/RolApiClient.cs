using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class RolApiClient
{
    private readonly HttpClient _http;

    public RolApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<RolDto>> ListarAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/roles", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<RolDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<List<PermisoDto>> ListarPermisosDisponiblesAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/roles/permisos-disponibles", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<PermisoDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<RolDto> CrearAsync(CrearRolDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/roles", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<RolDto>(cancellationToken: ct))!;
    }

    public async Task<RolDto> ActualizarAsync(int id, ActualizarRolDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/roles/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<RolDto>(cancellationToken: ct))!;
    }
}
