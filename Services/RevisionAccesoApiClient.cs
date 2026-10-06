using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

public class RevisionAccesoApiClient
{
    private readonly HttpClient _http;

    public RevisionAccesoApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<RevisionAccesoResumenDto>> ListarAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/revisiones-acceso", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<RevisionAccesoResumenDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<EstadoRevisionesAccesoDto> ObtenerEstadoAsync(CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync("api/revisiones-acceso/estado", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<EstadoRevisionesAccesoDto>(cancellationToken: ct))!;
    }

    public async Task<RevisionAccesoDetalleDto> ObtenerAsync(int id, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.GetAsync($"api/revisiones-acceso/{id}", ct), ct);

    public async Task<RevisionAccesoDetalleDto> CrearAsync(CrearRevisionAccesoDto dto, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PostAsJsonAsync("api/revisiones-acceso", dto, ct), ct);

    public async Task<RevisionAccesoDetalleDto> DecidirAsync(int id, int lineaId, DecidirAccesoDto dto, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PutAsJsonAsync($"api/revisiones-acceso/{id}/lineas/{lineaId}", dto, ct), ct);

    public async Task<RevisionAccesoDetalleDto> CerrarAsync(int id, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PostAsync($"api/revisiones-acceso/{id}/cerrar", null, ct), ct);

    public async Task<RevisionAccesoDetalleDto> CancelarAsync(int id, string motivo, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PostAsJsonAsync($"api/revisiones-acceso/{id}/cancelar", new CancelarRevisionAccesoDto(motivo), ct), ct);

    public async Task<(byte[] Contenido, string NombreArchivo)> DescargarActaAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/revisiones-acceso/{id}/acta", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        var contenido = await respuesta.Content.ReadAsByteArrayAsync(ct);
        var nombreArchivo = respuesta.Content.Headers.ContentDisposition?.FileNameStar?.Trim('"')
            ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "acta-revision-accesos.xlsx";
        return (contenido, nombreArchivo);
    }

    private static async Task<RevisionAccesoDetalleDto> LeerDetalleAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<RevisionAccesoDetalleDto>(cancellationToken: ct))!;
    }
}
