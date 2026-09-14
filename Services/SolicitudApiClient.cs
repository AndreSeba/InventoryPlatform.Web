using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class SolicitudApiClient
{
    private readonly HttpClient _http;

    public SolicitudApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<SolicitudDto>> ListarAsync(string? estado = null, CancellationToken ct = default)
    {
        var url = "api/solicitudes" + (string.IsNullOrWhiteSpace(estado) ? "" : $"?estado={estado}");
        var respuesta = await _http.GetAsync(url, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<SolicitudDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<SolicitudDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/solicitudes/{id}", ct);
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<SolicitudDto>(cancellationToken: ct);
    }

    public async Task<SolicitudDto> CrearAsync(CrearSolicitudDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/solicitudes", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<SolicitudDto>(cancellationToken: ct))!;
    }

    public async Task<SolicitudDto> AprobarAsync(int id, AprobarSolicitudDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync($"api/solicitudes/{id}/aprobar", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<SolicitudDto>(cancellationToken: ct))!;
    }

    public async Task<SolicitudDto> RechazarAsync(int id, RechazarSolicitudDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync($"api/solicitudes/{id}/rechazar", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<SolicitudDto>(cancellationToken: ct))!;
    }
}
