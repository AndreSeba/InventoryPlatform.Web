using System.Net.Http.Json;
using Inventory.Application.Dtos;

namespace Inventory.Web.Services;

public class UsuarioApiClient
{
    private readonly HttpClient _http;

    public UsuarioApiClient(HttpClient http) => _http = http;

    public async Task<List<UsuarioDto>> ListarAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/usuarios", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<UsuarioDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/usuarios", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<UsuarioDto>(cancellationToken: ct))!;
    }

    public async Task<UsuarioDto> ActualizarAsync(int id, ActualizarUsuarioDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/usuarios/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<UsuarioDto>(cancellationToken: ct))!;
    }
}
