using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Services;

internal static class ApiClientHelper
{
    public static async Task LanzarSiHayErrorAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        if (respuesta.IsSuccessStatusCode)
            return;

        // El 401 del handler de JWT bearer no siempre trae cuerpo (challenge estándar
        // sin ProblemDetails) — ReadFromJsonAsync sobre un cuerpo vacío/no-JSON tira
        // JsonException, así que no se puede confiar en que siempre haya un body válido.
        ProblemDetails? problema = null;
        try
        {
            problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: ct);
        }
        catch
        {
            // sin cuerpo o no es JSON — se usa el mensaje genérico de abajo
        }

        var mensaje = problema?.Detail ?? (respuesta.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Tu sesión expiró o no tenés acceso. Volvé a iniciar sesión.",
            HttpStatusCode.Forbidden => "No tenés permiso para hacer esto.",
            _ => "Ocurrió un error inesperado.",
        });

        throw new ApiException((int)respuesta.StatusCode, mensaje);
    }
}
