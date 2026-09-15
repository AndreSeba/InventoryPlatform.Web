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

    public async Task<(byte[] Contenido, string NombreArchivo)> GenerarHojaAsync(GenerarHojaConteoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/conteos/hoja", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        var contenido = await respuesta.Content.ReadAsByteArrayAsync(ct);
        var nombreArchivo = respuesta.Content.Headers.ContentDisposition?.FileNameStar?.Trim('"')
            ?? respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "ConteoFisico.xlsx";
        return (contenido, nombreArchivo);
    }

    public async Task<ImportarHojaConteoResultadoDto> ImportarHojaAsync(Stream archivo, string nombreArchivo, CancellationToken ct = default)
    {
        using var contenido = new MultipartFormDataContent();
        using var streamContent = new StreamContent(archivo);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        contenido.Add(streamContent, "archivo", nombreArchivo);

        var respuesta = await _http.PostAsync("api/conteos/importar", contenido, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ImportarHojaConteoResultadoDto>(cancellationToken: ct))!;
    }
}
