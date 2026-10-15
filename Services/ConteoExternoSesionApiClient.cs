using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

// Módulo de conteo independiente — sesión + conteo (Fase 3) y KPIs (Fase 4).
public class ConteoExternoSesionApiClient
{
    private readonly HttpClient _http;

    public ConteoExternoSesionApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<ConteoExternoSesionResumenDto>> ListarAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/conteo-externo/sesiones", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoExternoSesionResumenDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<ConteoExternoSesionDetalleDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteo-externo/sesiones/{id}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoSesionDetalleDto>(cancellationToken: ct))!;
    }

    public async Task<ConteoExternoSesionResumenDto> CrearAsync(CrearConteoExternoSesionDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/conteo-externo/sesiones", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoSesionResumenDto>(cancellationToken: ct))!;
    }

    public async Task RegistrarConteoAsync(int lineaId, int numeroConteo, RegistrarConteoExternoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/conteo-externo/sesiones/lineas/{lineaId}/conteos/{numeroConteo}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
    }

    // Transcripción ágil: la cantidad FINAL de la línea (Enter pasa a la siguiente en la pantalla).
    public async Task RegistrarCantidadFinalAsync(int lineaId, ConteoExternoCantidadFinalDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/conteo-externo/sesiones/lineas/{lineaId}/cantidad-final", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
    }

    // Producto sobrante: catálogo (si es nuevo) + existencia 0 + línea de la sesión, todo o nada.
    public async Task<ConteoExternoLineaDto> AgregarSobranteAsync(int sesionId, AgregarSobranteConteoExternoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync($"api/conteo-externo/sesiones/{sesionId}/sobrantes", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoLineaDto>(cancellationToken: ct))!;
    }

    public async Task ActualizarObservacionAsync(int lineaId, ActualizarObservacionConteoExternoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/conteo-externo/sesiones/lineas/{lineaId}/observacion", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
    }

    public async Task<ConteoExternoSesionResumenDto> CerrarAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsync($"api/conteo-externo/sesiones/{id}/cerrar", null, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoSesionResumenDto>(cancellationToken: ct))!;
    }

    public async Task<ConteoExternoSesionKpiDto> ObtenerKpisAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteo-externo/sesiones/{id}/kpis", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoExternoSesionKpiDto>(cancellationToken: ct))!;
    }

    public async Task<List<ConteoExternoSesionKpiDto>> ObtenerHistorialKpisAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/conteo-externo/sesiones/kpis/historial", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoExternoSesionKpiDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<(byte[] Contenido, string NombreArchivo)> DescargarKpisExcelAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteo-externo/sesiones/{id}/kpis/excel", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        var contenido = await respuesta.Content.ReadAsByteArrayAsync(ct);
        var nombreArchivo = respuesta.Content.Headers.ContentDisposition?.FileNameStar?.Trim('"')
            ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "ConteoExterno_KPIs.xlsx";
        return (contenido, nombreArchivo);
    }
}
