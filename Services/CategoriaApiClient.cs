using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class CategoriaApiClient
{
    private readonly HttpClient _http;

    public CategoriaApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<CategoriaDto>> ListarAsync(bool incluirInactivas = false, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/categorias?incluirInactivas={incluirInactivas}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<CategoriaDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<CategoriaDto> CrearAsync(CrearCategoriaDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/categorias", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<CategoriaDto>(cancellationToken: ct))!;
    }
}
