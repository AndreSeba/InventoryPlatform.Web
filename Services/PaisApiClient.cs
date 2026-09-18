using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

// El Bearer se agrega solo si hay sesión (mismo patrón que el resto de los ApiClient) —
// el login usa este mismo cliente SIN sesión, para poblar el selector de país con
// bandera antes de autenticarse (el endpoint Listar es [AllowAnonymous] en el backend).
public class PaisApiClient
{
    private readonly HttpClient _http;

    public PaisApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<PaisDto>> ListarAsync(bool incluirInactivos = false, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/paises?incluirInactivos={incluirInactivos}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<PaisDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<PaisDto> CrearAsync(CrearPaisDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/paises", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<PaisDto>(cancellationToken: ct))!;
    }

    public async Task<PaisDto> ActualizarAsync(int id, ActualizarPaisDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/paises/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<PaisDto>(cancellationToken: ct))!;
    }
}
