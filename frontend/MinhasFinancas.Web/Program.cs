using Fluxor;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MinhasFinancas.Web;
using MinhasFinancas.Web.Services;
using MinhasFinancas.Web.Store;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Base URL da API
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000/";

// AuthState provider
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());

// HTTP handler com JWT
builder.Services.AddTransient<AuthenticatedHttpHandler>();

// HttpClient autenticado para a API
builder.Services.AddHttpClient<AuthApiService>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<DashboardApiService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthenticatedHttpHandler>();

// MudBlazor
builder.Services.AddMudServices();

// Fluxor (State Management)
builder.Services.AddFluxor(options =>
    options.ScanAssemblies(typeof(Program).Assembly));

// Authorization
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
