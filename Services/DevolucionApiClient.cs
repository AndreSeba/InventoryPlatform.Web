using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Domain.Enums;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class DevolucionApiClient
{
    private readonly HttpClient _http;

    public DevolucionApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    // Todos los préstamos pendientes del país (para quien tiene movimientos.ver).
    public Task<List<PrestamoDto>> ListarPrestamosAsync(CancellationToken ct = default) => Obtener<PrestamoDto>("api/devoluciones/prestamos", ct);

    // Solo lo prestado a nombre del usuario que consulta.
    public Task<List<PrestamoDto>> ListarMisPrestamosAsync(CancellationToken ct = default) => Obtener<PrestamoDto>("api/devoluciones/prestamos/mios", ct);

    // Avisos del país (para el operario). Con `estado` = solo ese estado.
    public Task<List<AvisoDevolucionDto>> ListarAvisosAsync(EstadoAvisoDevolucion? estado = null, CancellationToken ct = default) =>
        Obtener<AvisoDevolucionDto>(estado is null ? "api/devoluciones/avisos" : $"api/devoluciones/avisos?estado={(int)estado}", ct);

    public Task<List<AvisoDevolucionDto>> ListarMisAvisosAsync(CancellationToken ct = default) => Obtener<AvisoDevolucionDto>("api/devoluciones/avisos/mios", ct);

    public async Task<AvisoDevolucionDto> AvisarAsync(CrearAvisoDevolucionDto dto, CancellationToken ct = default)
        => await Leer(await _http.PostAsJsonAsync("api/devoluciones/avisos", dto, ct), ct);

    public async Task<AvisoDevolucionDto> CancelarAvisoAsync(int id, string motivo, CancellationToken ct = default)
        => await Leer(await _http.PostAsJsonAsync($"api/devoluciones/avisos/{id}/cancelar", new CancelarAvisoDevolucionDto(motivo), ct), ct);

    private async Task<List<T>> Obtener<T>(string url, CancellationToken ct)
    {
        var respuesta = await _http.GetAsync(url, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<T>>(cancellationToken: ct) ?? [];
    }

    private static async Task<AvisoDevolucionDto> Leer(HttpResponseMessage respuesta, CancellationToken ct)
    {
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<AvisoDevolucionDto>(cancellationToken: ct))!;
    }
}
