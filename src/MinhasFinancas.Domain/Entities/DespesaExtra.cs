using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Domain.Entities;

public class DespesaExtra : Despesa
{
    public DateOnly DataDespesa { get; private set; }
    public FormaPagamentoDespesaExtra FormaPagamento { get; private set; }

    // EF Core
    private DespesaExtra() { }

    public static DespesaExtra Criar(
        Guid usuarioId,
        string descricao,
        decimal valor,
        DateOnly dataDespesa,
        string categoria,
        FormaPagamentoDespesaExtra formaPagamento)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));

        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));

        return new DespesaExtra
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Descricao = descricao.Trim(),
            ValorTotal = valor,
            DataDespesa = dataDespesa,
            Categoria = categoria,
            FormaPagamento = formaPagamento,
            TipoDespesa = TipoDespesa.Extra,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Atualizar(string descricao, decimal valor, DateOnly dataDespesa, string categoria, FormaPagamentoDespesaExtra formaPagamento)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));
        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));

        Descricao = descricao.Trim();
        ValorTotal = valor;
        DataDespesa = dataDespesa;
        Categoria = categoria;
        FormaPagamento = formaPagamento;
    }
}
