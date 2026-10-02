using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class InicioApiClient
{
    private readonly HttpClient _http;

    public InicioApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<ResumenInicioDto> ObtenerResumenAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/inicio/resumen", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ResumenInicioDto>(cancellationToken: ct))!;
    }
}
