using System.Diagnostics;

namespace Inventory.Web.Services;

// Mide cuánto tarda cada llamada a la API y lo deja en la consola del frontend. Sirve para
// separar "la API tarda" de "la página tarda" cuando algo se siente lento. Solo usa ILogger
// (singleton), nada scoped: el handler del IHttpClientFactory vive en un scope propio y NO
// puede pedir AuthState (ver el comentario de Program.cs).
public class ApiTimingHandler(ILogger<ApiTimingHandler> logger) : DelegatingHandler
{
    private const long UmbralLentoMs = 1000;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var reloj = Stopwatch.StartNew();
        try
        {
            var respuesta = await base.SendAsync(request, ct);
            var ms = reloj.ElapsedMilliseconds;
            logger.Log(ms >= UmbralLentoMs ? LogLevel.Warning : LogLevel.Information,
                "API {Metodo} {Ruta} -> {Estado} en {Ms} ms", request.Method, request.RequestUri?.PathAndQuery, (int)respuesta.StatusCode, ms);
            return respuesta;
        }
        catch (Exception ex)
        {
            logger.LogWarning("API {Metodo} {Ruta} FALLÓ tras {Ms} ms: {Mensaje}",
                request.Method, request.RequestUri?.PathAndQuery, reloj.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}
