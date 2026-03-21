using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MinhasFinancas.API.Middlewares;
using MinhasFinancas.Application;
using MinhasFinancas.Infrastructure;
using MinhasFinancas.Infrastructure.Data;
using Serilog;
using Serilog.Events;

// ======================================================
// SERILOG — Configurar antes do builder para capturar
// erros de startup
// ======================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando MinhasFinancas API...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog como provider de logging
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .WriteTo.Console());

    // ======================================================
    // SERVIÇOS
    // ======================================================
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // CORS — permitir frontend Blazor com credenciais
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("BlazorPolicy", policy =>
        {
            var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]?.Split(',')
                ?? ["http://localhost", "http://localhost:80", "http://localhost:5001"];

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Necessário para cookies HttpOnly
        });
    });

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Minhas Finanças API",
            Version = "v1",
            Description = "API de controle de finanças pessoais"
        });

        // Adicionar botão de autenticação JWT no Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Example: 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // ======================================================
    // PIPELINE
    // ======================================================

    // Auto-migration em desenvolvimento
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minhas Finanças v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors("BlazorPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API encerrada inesperadamente.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
