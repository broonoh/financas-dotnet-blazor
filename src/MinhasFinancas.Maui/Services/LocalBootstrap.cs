using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Maui.Services;

/// <summary>
/// Garante que o banco local exista e que haja um Usuario "dono" do aparelho.
/// Sem login: cada dispositivo tem exatamente um usuário, criado automaticamente
/// no primeiro uso.
/// </summary>
public static class LocalBootstrap
{
    private const string EmailLocal = "local@minhasfinancas.app";

    public static string CaminhoBanco =>
        Path.Combine(FileSystem.AppDataDirectory, "minhas_financas.db3");

    public static async Task<Guid> GarantirUsuarioLocalAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await AtualizarSchemaAsync(db);

        var usuarioRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var senhaService = scope.ServiceProvider.GetRequiredService<ISenhaService>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Busca por "o único usuário local" em vez de por e-mail fixo: o e-mail
        // agora é editável na tela de Perfil, então não pode mais ser a chave de busca
        // (senão, ao editá-lo, o próximo boot criaria um segundo usuário "fantasma"
        // e todos os dados existentes ficariam órfãos, presos ao usuário antigo).
        var usuario = await usuarioRepo.ObterPrimeiroAsync();
        if (usuario is not null)
            return usuario.Id;

        usuario = Usuario.Criar(
            nome: "Usuário",
            email: EmailLocal,
            senhaHash: senhaService.HashSenha(Guid.NewGuid().ToString()),
            dataNascimento: new DateOnly(2000, 1, 1));

        await usuarioRepo.AdicionarAsync(usuario);
        await uow.CommitAsync();

        return usuario.Id;
    }

    /// <summary>
    /// EnsureCreatedAsync só cria o schema na primeira vez; em bancos locais já existentes
    /// (aparelho já em uso), colunas/tabelas novas adicionadas ao modelo depois não aparecem.
    /// Aqui aplicamos essas diferenças manualmente, sem tocar nos dados já gravados.
    /// </summary>
    private static async Task AtualizarSchemaAsync(AppDbContext db)
    {
        await db.Database.OpenConnectionAsync();
        try
        {
            var conn = db.Database.GetDbConnection();

            bool ExisteTabela(string nome)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=$nome";
                var p = cmd.CreateParameter();
                p.ParameterName = "$nome";
                p.Value = nome;
                cmd.Parameters.Add(p);
                return cmd.ExecuteScalar() is not null;
            }

            bool ExisteColuna(string tabela, string coluna)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"PRAGMA table_info({tabela})";
                using var reader = cmd.ExecuteReader();
                var idxNome = -1;
                while (reader.Read())
                {
                    if (idxNome < 0) idxNome = reader.GetOrdinal("name");
                    if (string.Equals(reader.GetString(idxNome), coluna, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }

            if (!ExisteTabela("credores"))
            {
                await db.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE credores (
                        id TEXT NOT NULL PRIMARY KEY,
                        usuario_id TEXT NOT NULL,
                        nome TEXT NOT NULL,
                        data_criacao TEXT NOT NULL
                    );
                    """);
                await db.Database.ExecuteSqlRawAsync(
                    "CREATE UNIQUE INDEX idx_credores_usuario_nome ON credores (usuario_id, nome);");
            }

            if (!ExisteColuna("despesas", "credor_id"))
            {
                await db.Database.ExecuteSqlRawAsync("ALTER TABLE despesas ADD COLUMN credor_id TEXT NULL;");
                await db.Database.ExecuteSqlRawAsync(
                    "CREATE INDEX idx_despesas_credor ON despesas (credor_id);");
            }

            if (!ExisteTabela("formas_pagamento"))
            {
                // Os enums antigos gravavam o NOME do membro C# (ex: "CartaoCredito"), não um
                // rótulo amigável. Agora que o campo é texto livre, convertemos os valores já
                // gravados neste aparelho para os rótulos que sempre foram exibidos na tela.
                await db.Database.ExecuteSqlRawAsync("""
                    UPDATE despesas SET forma_pagamento = CASE forma_pagamento
                        WHEN 'CartaoCredito' THEN 'Cartão de Crédito'
                        WHEN 'PixParcelado' THEN 'Pix Parcelado'
                        WHEN 'BoletoParcelado' THEN 'Boleto Parcelado'
                        ELSE forma_pagamento
                    END
                    WHERE forma_pagamento IS NOT NULL;
                    """);

                await db.Database.ExecuteSqlRawAsync("""
                    UPDATE despesas SET forma_pagamento_extra = CASE forma_pagamento_extra
                        WHEN 'CartaoCredito' THEN 'Cartão de Crédito'
                        WHEN 'Pix' THEN 'Pix'
                        WHEN 'Dinheiro' THEN 'Dinheiro'
                        WHEN 'Boleto' THEN 'Boleto'
                        ELSE forma_pagamento_extra
                    END
                    WHERE forma_pagamento_extra IS NOT NULL;
                    """);

                await db.Database.ExecuteSqlRawAsync("""
                    CREATE TABLE formas_pagamento (
                        id TEXT NOT NULL PRIMARY KEY,
                        usuario_id TEXT NOT NULL,
                        nome TEXT NOT NULL,
                        data_criacao TEXT NOT NULL
                    );
                    """);
                await db.Database.ExecuteSqlRawAsync(
                    "CREATE UNIQUE INDEX idx_formas_pagamento_usuario_nome ON formas_pagamento (usuario_id, nome);");

                // Backfill: popula o cadastro com os valores já usados neste aparelho.
                // Inserido via EF (não SQL bruto) para que Guid/DateTime sejam serializados
                // exatamente como o provider Sqlite espera ao ler de volta depois.
                var distintos = new List<(string UsuarioId, string Nome)>();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = """
                        SELECT DISTINCT usuario_id, forma_pagamento AS nome FROM despesas WHERE forma_pagamento IS NOT NULL
                        UNION
                        SELECT DISTINCT usuario_id, forma_pagamento_extra AS nome FROM despesas WHERE forma_pagamento_extra IS NOT NULL
                        """;
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        distintos.Add((reader.GetString(0), reader.GetString(1)));
                }

                foreach (var (usuarioId, nome) in distintos)
                    db.FormasPagamento.Add(FormaPagamento.Criar(Guid.Parse(usuarioId), nome));

                if (distintos.Count > 0)
                    await db.SaveChangesAsync();
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
