using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class FirmaApiClient
{
    private readonly HttpClient _http;

    public FirmaApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<FirmaPropiaDto> ObtenerAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/firma", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<FirmaPropiaDto>(cancellationToken: ct))!;
    }

    public async Task<FirmaPropiaDto> GuardarAsync(GuardarFirmaDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync("api/firma", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<FirmaPropiaDto>(cancellationToken: ct))!;
    }

    public async Task EliminarAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.DeleteAsync("api/firma", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
    }
}
