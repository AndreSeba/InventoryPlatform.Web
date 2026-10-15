using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

// Módulo de conteo independiente — importación desde Excel externo (Fase 5).
public class ConteoExternoImportacionApiClient
{
    private readonly HttpClient _http;

    public ConteoExternoImportacionApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<VistaPreviaImportacionConteoExternoDto> VistaPreviaAsync(Stream archivo, string nombreArchivo, CancellationToken ct = default)
    {
        using var contenido = ArmarArchivo(archivo, nombreArchivo);
        var respuesta = await _http.PostAsync("api/conteo-externo/importacion/vista-previa", contenido, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<VistaPreviaImportacionConteoExternoDto>(cancellationToken: ct))!;
    }

    public async Task<ImportacionConteoExternoResultadoDto> ConfirmarAsync(Stream archivo, string nombreArchivo, MapeoColumnasConteoExternoDto mapeo, CancellationToken ct = default)
    {
        using var contenido = ArmarArchivo(archivo, nombreArchivo);
        contenido.Add(new StringContent(mapeo.FilaInicioDatos.ToString()), "mapeo.FilaInicioDatos");
        contenido.Add(new StringContent(mapeo.ColCodigo.ToString()), "mapeo.ColCodigo");
        if (mapeo.ColNombre is not null) contenido.Add(new StringContent(mapeo.ColNombre.Value.ToString()), "mapeo.ColNombre");
        if (mapeo.ColFamilia is not null) contenido.Add(new StringContent(mapeo.ColFamilia.Value.ToString()), "mapeo.ColFamilia");
        contenido.Add(new StringContent(mapeo.ColAlmacen.ToString()), "mapeo.ColAlmacen");
        if (mapeo.ColLote is not null) contenido.Add(new StringContent(mapeo.ColLote.Value.ToString()), "mapeo.ColLote");
        contenido.Add(new StringContent(mapeo.ColCantidad.ToString()), "mapeo.ColCantidad");
        if (mapeo.ColUnidad is not null) contenido.Add(new StringContent(mapeo.ColUnidad.Value.ToString()), "mapeo.ColUnidad");
        if (mapeo.ColCosto is not null) contenido.Add(new StringContent(mapeo.ColCosto.Value.ToString()), "mapeo.ColCosto");
        contenido.Add(new StringContent(mapeo.CrearProductosNuevos.ToString()), "mapeo.CrearProductosNuevos");

        var respuesta = await _http.PostAsync("api/conteo-externo/importacion/confirmar", contenido, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ImportacionConteoExternoResultadoDto>(cancellationToken: ct))!;
    }

    private static MultipartFormDataContent ArmarArchivo(Stream archivo, string nombreArchivo)
    {
        var contenido = new MultipartFormDataContent();
        var streamContent = new StreamContent(archivo);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        contenido.Add(streamContent, "archivo", nombreArchivo);
        return contenido;
    }
}
