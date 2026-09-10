using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MinhasFinancas.Application;
using MinhasFinancas.Infrastructure;
using MinhasFinancas.Maui.Services;
using MinhasFinancas.Maui.ViewModels;
using MinhasFinancas.Maui.Views;

namespace MinhasFinancas.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddApplication();
        builder.Services.AddInfrastructureLocal(LocalBootstrap.CaminhoBanco);

        builder.Services.AddSingleton<UsuarioContexto>();
        builder.Services.AddSingleton<PdfService>();

        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ParcelasViewModel>();
        builder.Services.AddTransient<GraficosViewModel>();
        builder.Services.AddTransient<ReceitasViewModel>();
        builder.Services.AddTransient<DespesasViewModel>();
        builder.Services.AddTransient<DevedoresViewModel>();
        builder.Services.AddTransient<CredoresViewModel>();
        builder.Services.AddTransient<FormasPagamentoViewModel>();
        builder.Services.AddTransient<CategoriasViewModel>();
        builder.Services.AddTransient<ResumoMensalViewModel>();
        builder.Services.AddTransient<PerfilViewModel>();

        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<ParcelasPage>();
        builder.Services.AddTransient<GraficosPage>();
        builder.Services.AddTransient<ReceitasPage>();
        builder.Services.AddTransient<DespesasPage>();
        builder.Services.AddTransient<DevedoresPage>();
        builder.Services.AddTransient<CredoresPage>();
        builder.Services.AddTransient<FormasPagamentoPage>();
        builder.Services.AddTransient<CategoriasPage>();
        builder.Services.AddTransient<ResumoMensalPage>();
        builder.Services.AddTransient<PerfilPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Roda em thread de pool para não travar a UI thread durante o bootstrap
        // (evita deadlock/ANR na inicialização do app).
        var usuarioContexto = app.Services.GetRequiredService<UsuarioContexto>();
        Task.Run(async () =>
        {
            usuarioContexto.UsuarioId = await LocalBootstrap.GarantirUsuarioLocalAsync(app.Services);
        }).GetAwaiter().GetResult();

        return app;
    }
}
