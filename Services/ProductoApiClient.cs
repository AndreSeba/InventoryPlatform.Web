using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Application.Dtos;
using Inventory.Web.Services.Auth;

namespace Inventory.Web.Services;

// "Servicio de cliente": encapsula el HttpClient hacia la Web API, sin
// lógica de negocio propia. Los componentes .razor lo inyectan con @inject.
public class ProductoApiClient
{
    private readonly HttpClient _http;

    public ProductoApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        if (authState.IsAuthenticated)
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authState.Token);
    }

    public async Task<List<ProductoDto>> ListarAsync(int? categoriaId = null, bool incluirInactivos = false, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (categoriaId is not null) query.Add($"categoriaId={categoriaId}");
        if (incluirInactivos) query.Add("incluirInactivos=true");

        var url = "api/productos" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        var respuesta = await _http.GetAsync(url, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<List<ProductoDto>>(cancellationToken: ct) ?? [];
    }

    public async Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/productos/{id}", ct);
        if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return await respuesta.Content.ReadFromJsonAsync<ProductoDto>(cancellationToken: ct);
    }

    public async Task<ProductoDto> CrearAsync(CrearProductoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PostAsJsonAsync("api/productos", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ProductoDto>(cancellationToken: ct))!;
    }

    public async Task<ProductoDto> ActualizarAsync(int id, ActualizarProductoDto dto, CancellationToken ct = default)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/productos/{id}", dto, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        return (await respuesta.Content.ReadFromJsonAsync<ProductoDto>(cancellationToken: ct))!;
    }

    public async Task DesactivarAsync(int id, CancellationToken ct = default)
    {
        var respuesta = await _http.DeleteAsync($"api/productos/{id}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
    }

    public async Task<string> SubirImagenAsync(Stream contenido, string nombreArchivo, string contentType, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        using var streamContent = new StreamContent(contenido);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(streamContent, "archivo", nombreArchivo);

        var respuesta = await _http.PostAsync("api/productos/imagen", form, ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        var resultado = await respuesta.Content.ReadFromJsonAsync<ImagenSubidaDto>(cancellationToken: ct);
        return resultado!.Url;
    }
    public async Task<string> ObtenerSiguienteCodigoAsync(int categoriaId, CancellationToken ct = default)
    {
        var respuesta = await _http.GetAsync($"api/productos/siguiente-codigo?categoriaId={categoriaId}", ct);
        await ApiClientHelper.LanzarSiHayErrorAsync(respuesta, ct);
        var resultado = await respuesta.Content.ReadFromJsonAsync<SiguienteCodigoDto>(cancellationToken: ct);
        return resultado!.Codigo;
    }
}
