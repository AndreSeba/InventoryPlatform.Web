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
// servidor, nunca lo manda al navegador. AuthHeaderHandler lo adjunta como Bearer
// a cada *ApiClient real (Login usa AuthApiClient, sin este handler, a propósito:
// todavía no hay token que mandar).
builder.Services.AddScoped<AuthState>();
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddHttpClient<AuthApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient<CategoriaApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<AreaApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<UbicacionApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<ProductoApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<MovimientoApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<SolicitudApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<ConteoApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<UsuarioApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddHttpClient<RolApiClient>(client => client.BaseAddress = new Uri(apiBaseUrl)).AddHttpMessageHandler<AuthHeaderHandler>();

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
