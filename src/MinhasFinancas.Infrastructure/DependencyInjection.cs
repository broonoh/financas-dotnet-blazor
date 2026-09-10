using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;
using MinhasFinancas.Infrastructure.Repositories;
using MinhasFinancas.Infrastructure.Services;

namespace MinhasFinancas.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Infraestrutura completa para a API: PostgreSQL + repositórios + serviços de
    /// autenticação (token/senha/rate-limit). A autenticação JWT (ASP.NET Core) é
    /// registrada separadamente no Program.cs da API, não aqui — este projeto também
    /// é referenciado pelo app MAUI local, que não pode depender do ASP.NET Core.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

        AddRepositories(services);

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ISenhaService, SenhaService>();
        services.AddSingleton<LoginAttemptService>(); // Rate limiting em memória

        return services;
    }

    /// <summary>
    /// Infraestrutura local para o app MAUI: SQLite em arquivo no dispositivo,
    /// sem autenticação/JWT (o app é single-user e offline por dispositivo).
    /// </summary>
    public static IServiceCollection AddInfrastructureLocal(
        this IServiceCollection services,
        string dbPath)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        AddRepositories(services);

        services.AddScoped<ISenhaService, SenhaService>();

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IReceitaRepository, ReceitaRepository>();
        services.AddScoped<IDespesaRepository, DespesaRepository>();
        services.AddScoped<IParcelaRepository, ParcelaRepository>();
        services.AddScoped<IDividaRepository, DividaRepository>();
        services.AddScoped<IDevedorRepository, DevedorRepository>();
        services.AddScoped<ICredorRepository, CredorRepository>();
        services.AddScoped<ICategoriaReceitaRepository, CategoriaReceitaRepository>();
        services.AddScoped<ICategoriaDespesaRepository, CategoriaDespesaRepository>();
        services.AddScoped<IFormaPagamentoRepository, FormaPagamentoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
