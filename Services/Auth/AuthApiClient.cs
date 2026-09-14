using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services;

namespace Inventory.Web.Services.Auth;

// Sin AuthHeaderHandler a propósito: para loguearse todavía no hay token que mandar.
public class AuthApiClient
{
    private readonly HttpClient _http;

    public AuthApiClient(HttpClient http) => _http = http;

    public async Task<LoginResultDto> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/auth/login", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<LoginResultDto>(cancellationToken: ct))!;
    }
}
