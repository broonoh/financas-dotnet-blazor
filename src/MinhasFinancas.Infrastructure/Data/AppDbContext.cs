using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Enums;
using MinhasFinancas.Domain.ValueObjects;

namespace MinhasFinancas.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Receita> Receitas => Set<Receita>();
    public DbSet<Despesa> Despesas => Set<Despesa>();
    public DbSet<DespesaFixa> DespesasFixas => Set<DespesaFixa>();
    public DbSet<DespesaExtra> DespesasExtras => Set<DespesaExtra>();
    public DbSet<Parcela> Parcelas => Set<Parcela>();
    public DbSet<Divida> Dividas => Set<Divida>();
    public DbSet<ParcelaDivida> ParcelasDivida => Set<ParcelaDivida>();
    public DbSet<CategoriaReceita> CategoriasReceita => Set<CategoriaReceita>();
    public DbSet<CategoriaDespesa> CategoriasDespesa => Set<CategoriaDespesa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================
        // USUARIO
        // ==========================
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(100).IsRequired();
            entity.Property(e => e.DataNascimento).HasColumnName("data_nascimento").IsRequired();
            entity.Property(e => e.Telefone).HasColumnName("telefone").HasMaxLength(20);
            entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            entity.Property(e => e.TwoFactorSecret).HasColumnName("two_factor_secret").HasMaxLength(100);
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token").HasMaxLength(500);
            entity.Property(e => e.RefreshTokenExpiry).HasColumnName("refresh_token_expiry");
            entity.Property(e => e.DataCadastro).HasColumnName("data_cadastro");
            entity.Property(e => e.Ativo).HasColumnName("ativo");

            // Owned: Email value object
            entity.OwnsOne(e => e.Email, owned =>
            {
                owned.Property(v => v.Valor).HasColumnName("email").HasMaxLength(100).IsRequired();
                owned.HasIndex(v => v.Valor).IsUnique();
            });

            // Owned: SenhaHash value object
            entity.OwnsOne(e => e.SenhaHash, owned =>
            {
                owned.Property(v => v.Valor).HasColumnName("senha_hash").HasMaxLength(255).IsRequired();
            });
        });

        // ==========================
        // RECEITA
        // ==========================
        modelBuilder.Entity<Receita>(entity =>
        {
            entity.ToTable("receitas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id").IsRequired();
            entity.Property(e => e.Descricao).HasColumnName("descricao").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Valor).HasColumnName("valor").HasColumnType("decimal(15,2)").IsRequired();
            entity.Property(e => e.DataRecebimento).HasColumnName("data_recebimento").IsRequired();
            entity.Property(e => e.Categoria).HasColumnName("categoria").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Recorrente).HasColumnName("recorrente");
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");

            entity.HasIndex(e => new { e.UsuarioId, e.DataRecebimento }).HasDatabaseName("idx_receitas_usuario_data");
        });

        // ==========================
        // DESPESA (TPH - Table-Per-Hierarchy)
        // ==========================
        modelBuilder.Entity<Despesa>(entity =>
        {
            entity.ToTable("despesas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id").IsRequired();
            entity.Property(e => e.Descricao).HasColumnName("descricao").HasMaxLength(100).IsRequired();
            entity.Property(e => e.ValorTotal).HasColumnName("valor_total").HasColumnType("decimal(15,2)").IsRequired();
            entity.Property(e => e.Categoria).HasColumnName("categoria").HasMaxLength(50).IsRequired();
            entity.Property(e => e.TipoDespesa).HasColumnName("tipo").HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");

            // Discriminator column for TPH
            entity.HasDiscriminator(e => e.TipoDespesa)
                .HasValue<DespesaFixa>(TipoDespesa.Fixa)
                .HasValue<DespesaExtra>(TipoDespesa.Extra);
        });

        modelBuilder.Entity<DespesaFixa>(entity =>
        {
            entity.Property(e => e.QuantidadeParcelas).HasColumnName("quantidade_parcelas");
            entity.Property(e => e.DataPrimeiraParcela).HasColumnName("data_primeira_parcela");
            entity.Property(e => e.FormaPagamento).HasColumnName("forma_pagamento").HasConversion<string>().HasMaxLength(30);

            entity.HasMany(e => e.Parcelas)
                .WithOne()
                .HasForeignKey(p => p.DespesaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DespesaExtra>(entity =>
        {
            entity.Property(e => e.DataDespesa).HasColumnName("data_despesa");
            entity.Property(e => e.FormaPagamento).HasColumnName("forma_pagamento_extra").HasConversion<string>().HasMaxLength(30);
        });

        // ==========================
        // PARCELA
        // ==========================
        modelBuilder.Entity<Parcela>(entity =>
        {
            entity.ToTable("parcelas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DespesaId).HasColumnName("despesa_id").IsRequired();
            entity.Property(e => e.Numero).HasColumnName("numero").IsRequired();
            entity.Property(e => e.Valor).HasColumnName("valor").HasColumnType("decimal(15,2)").IsRequired();
            entity.Property(e => e.DataVencimento).HasColumnName("data_vencimento").IsRequired();
            entity.Property(e => e.Paga).HasColumnName("paga");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");

            entity.HasIndex(e => e.DataVencimento).HasDatabaseName("idx_parcelas_vencimento");
        });

        // ==========================
        // DIVIDA
        // ==========================
        modelBuilder.Entity<Divida>(entity =>
        {
            entity.ToTable("dividas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id").IsRequired();
            entity.Property(e => e.NomeDevedor).HasColumnName("nome_devedor").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descricao).HasColumnName("descricao").HasMaxLength(200).IsRequired();
            entity.Property(e => e.ValorTotal).HasColumnName("valor_total").HasColumnType("decimal(15,2)").IsRequired();
            entity.Property(e => e.QuantidadeParcelas).HasColumnName("quantidade_parcelas").IsRequired();
            entity.Property(e => e.DataCompra).HasColumnName("data_compra").IsRequired();
            entity.Property(e => e.Ativa).HasColumnName("ativa");
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");

            entity.HasMany(e => e.Parcelas)
                .WithOne()
                .HasForeignKey(p => p.DividaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UsuarioId).HasDatabaseName("idx_dividas_usuario");
        });

        // ==========================
        // PARCELA DIVIDA
        // ==========================
        modelBuilder.Entity<ParcelaDivida>(entity =>
        {
            entity.ToTable("parcelas_divida");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DividaId).HasColumnName("divida_id").IsRequired();
            entity.Property(e => e.Numero).HasColumnName("numero").IsRequired();
            entity.Property(e => e.Valor).HasColumnName("valor").HasColumnType("decimal(15,2)").IsRequired();
            entity.Property(e => e.DataVencimento).HasColumnName("data_vencimento").IsRequired();
            entity.Property(e => e.Paga).HasColumnName("paga");
            entity.Property(e => e.DataPagamento).HasColumnName("data_pagamento");
        });

        // ==========================
        // CATEGORIA RECEITA
        // ==========================
        modelBuilder.Entity<CategoriaReceita>(entity =>
        {
            entity.ToTable("categorias_receita");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id").IsRequired();
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");

            entity.HasIndex(e => new { e.UsuarioId, e.Nome }).IsUnique().HasDatabaseName("idx_categorias_receita_usuario_nome");
        });

        // ==========================
        // CATEGORIA DESPESA
        // ==========================
        modelBuilder.Entity<CategoriaDespesa>(entity =>
        {
            entity.ToTable("categorias_despesa");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id").IsRequired();
            entity.Property(e => e.Nome).HasColumnName("nome").HasMaxLength(50).IsRequired();
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");

            entity.HasIndex(e => new { e.UsuarioId, e.Nome }).IsUnique().HasDatabaseName("idx_categorias_despesa_usuario_nome");
        });
    }
}
