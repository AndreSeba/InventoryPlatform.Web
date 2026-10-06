using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class NotificacionApiClient
{
    private readonly HttpClient _http;

    public NotificacionApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<NotificacionesDto> ListarAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/notificaciones", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<NotificacionesDto>(cancellationToken: ct))!;
    }

    public async Task MarcarLeidaAsync(long id, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsync($"api/notificaciones/{id}/leida", null, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
    }

    public async Task MarcarTodasLeidasAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsync("api/notificaciones/leidas", null, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
    }
}
