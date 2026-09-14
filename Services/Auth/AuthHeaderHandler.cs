using System.Net.Http.Headers;

namespace Inventory.Web.Services.Auth;

// DelegatingHandler adjunto a los HttpClient tipados (ProductoApiClient, etc.) vía
// .AddHttpMessageHandler<AuthHeaderHandler>() — agrega el Bearer token de la sesión
// actual a cada request, sin que cada *ApiClient tenga que saber nada de auth.
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly AuthState _authState;

    public AuthHeaderHandler(AuthState authState) => _authState = authState;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (_authState.IsAuthenticated)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);

        return base.SendAsync(request, ct);
    }
}
