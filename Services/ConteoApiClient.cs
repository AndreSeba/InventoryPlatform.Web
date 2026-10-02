using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Domain.Enums;
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

    public async Task<List<ConteoResumenDto>> ListarAsync(EstadoConteo? estado = null, CancellationToken ct = default)
    {
        var url = estado is null ? "api/conteos" : $"api/conteos?estado={(int)estado}";
        var respuesta = await _http.GetAsync(url, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ConteoResumenDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<ConteoDetalleDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteos/{id}", ct);
        return await LeerDetalleAsync(respuesta, ct);
    }

    public async Task<ConteoDetalleDto> CrearAsync(CrearConteoDto dto, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PostAsJsonAsync("api/conteos", dto, ct), ct);

    public async Task<ConteoDetalleDto> GuardarCantidadesAsync(int id, GuardarCantidadesDto dto, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PutAsJsonAsync($"api/conteos/{id}/cantidades", dto, ct), ct);

    public async Task<(byte[] Contenido, string NombreArchivo)> DescargarHojaAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteos/{id}/hoja", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        var contenido = await respuesta.Content.ReadAsByteArrayAsync(ct);
        var nombreArchivo = respuesta.Content.Headers.ContentDisposition?.FileNameStar?.Trim('"')
            ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "ConteoFisico.xlsx";
        return (contenido, nombreArchivo);
    }

    public async Task<ImportarHojaConteoResultadoDto> ImportarHojaAsync(int id, Stream archivo, string nombreArchivo, bool adjuntarComoEvidencia, CancellationToken ct = default)
    {
        using var contenido = ArmarMultipart(archivo, nombreArchivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        var respuesta = await _http.PostAsync($"api/conteos/{id}/importar?adjuntar={(adjuntarComoEvidencia ? "true" : "false")}", contenido, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ImportarHojaConteoResultadoDto>(cancellationToken: ct))!;
    }

    public async Task<ConteoEvidenciaDto> AdjuntarEvidenciaAsync(int id, Stream archivo, string nombreArchivo, string contentType, CancellationToken ct = default)
    {
        using var contenido = ArmarMultipart(archivo, nombreArchivo, contentType);
        var respuesta = await _http.PostAsync($"api/conteos/{id}/evidencias", contenido, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoEvidenciaDto>(cancellationToken: ct))!;
    }

    public async Task EliminarEvidenciaAsync(int id, int evidenciaId, CancellationToken ct = default)
    {
        var respuesta = await _http.DeleteAsync($"api/conteos/{id}/evidencias/{evidenciaId}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
    }

    public async Task<(byte[] Datos, string ContentType)> ObtenerEvidenciaAsync(int id, int evidenciaId, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/conteos/{id}/evidencias/{evidenciaId}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        var datos = await respuesta.Content.ReadAsByteArrayAsync(ct);
        return (datos, respuesta.Content.Headers.ContentType?.MediaType ?? "application/octet-stream");
    }

    public async Task<ConteoDetalleDto> CerrarAsync(int id, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PostAsync($"api/conteos/{id}/cerrar", null, ct), ct);

    public async Task<ConteoDetalleDto> CancelarAsync(int id, string motivo, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PostAsJsonAsync($"api/conteos/{id}/cancelar", new CancelarConteoDto(motivo), ct), ct);

    public async Task<ConteoDetalleDto> CrearReconteoAsync(int id, CancellationToken ct = default)
        => await LeerDetalleAsync(await _http.PostAsync($"api/conteos/{id}/reconteo", null, ct), ct);

    private static MultipartFormDataContent ArmarMultipart(Stream archivo, string nombreArchivo, string contentType)
    {
        var contenido = new MultipartFormDataContent();
        var streamContent = new StreamContent(archivo);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        contenido.Add(streamContent, "archivo", nombreArchivo);
        return contenido;
    }

    private static async Task<ConteoDetalleDto> LeerDetalleAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ConteoDetalleDto>(cancellationToken: ct))!;
    }
}
