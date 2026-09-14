using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class MovimientoApiClient
{
    private readonly HttpClient _http;

    public MovimientoApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<MovimientoDto>> ListarAsync(DateTime? desde = null, DateTime? hasta = null, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (desde is not null) query.Add($"desde={desde:yyyy-MM-dd}");
        if (hasta is not null) query.Add($"hasta={hasta:yyyy-MM-dd}");
        var url = "api/movimientos" + (query.Count > 0 ? "?" + string.Join("&", query) : "");

        var respuesta = await _http.GetAsync(url, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<MovimientoDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<List<MovimientoDto>> ListarPorProductoAsync(int productoId, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/movimientos/producto/{productoId}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<MovimientoDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<List<MovimientoDto>> ListarPrestamosPendientesAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/movimientos/prestamos-pendientes", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<MovimientoDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<MovimientoResultadoDto> RegistrarEntradaAsync(RegistrarEntradaDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/movimientos/entradas", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<MovimientoResultadoDto>(cancellationToken: ct))!;
    }

    public async Task<MovimientoResultadoDto> RegistrarSalidaAsync(RegistrarSalidaDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/movimientos/salidas", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<MovimientoResultadoDto>(cancellationToken: ct))!;
    }

    public async Task<MovimientoResultadoDto> RegistrarAjusteAsync(RegistrarAjusteDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/movimientos/ajustes", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<MovimientoResultadoDto>(cancellationToken: ct))!;
    }

    public async Task<MovimientoResultadoDto> RegistrarDevolucionAsync(RegistrarDevolucionDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/movimientos/devoluciones", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<MovimientoResultadoDto>(cancellationToken: ct))!;
    }
}
