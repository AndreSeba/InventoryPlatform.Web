using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class ConteoApiClient
{
    private readonly HttpClient _http;

    public ConteoApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<ConteoDto>> ListarPorSesionAsync(string sesionConteo, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteos/sesion/{Uri.EscapeDataString(sesionConteo)}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<ConteoDto> RegistrarAsync(RegistrarConteoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/conteos", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoDto>(cancellationToken: ct))!;
    }
}
