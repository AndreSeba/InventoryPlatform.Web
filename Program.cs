using Inventory.Web.Components;
using Inventory.Web.Services;
using Inventory.Web.Services.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Cliente HTTP hacia la Web API — este frontend nunca accede a la base de
// datos directamente, solo consume la API real de InventoryPlatform.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Falta 'ApiBaseUrl' en la configuración.");

// AuthState: scoped, vive por circuito (por pestaña) — guarda el JWT del lado del
// servidor, nunca lo manda al navegador. Cada *ApiClient autenticado recibe AuthState
// directo en su constructor y fija el Bearer ahí (NO vía DelegatingHandler +
// AddHttpMessageHandler: ese pipeline lo construye IHttpClientFactory en un scope
// interno propio, no en el del circuito, así que un handler ahí nunca ve el AuthState
// real del usuario — swallowed 401 en todo el CRUD real hasta que se detectó con la
// app corriendo). Login usa AuthApiClient sin token, a propósito: todavía no hay uno.
builder.Services.AddScoped<AuthState>();
builder.Services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<CategoriaApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<AreaApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<UbicacionApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ProductoApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<MovimientoApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<SolicitudApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ConteoApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<UsuarioApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<RolApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
