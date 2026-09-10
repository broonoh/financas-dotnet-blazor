using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Domain.Entities;

public class DespesaExtra : Despesa
{
    private const string FormaPagamentoCartaoCredito = "Cartão de Crédito";

    public DateOnly DataDespesa { get; private set; }
    public string FormaPagamento { get; private set; } = string.Empty;
    public DateOnly? PagaEm { get; private set; }
    public bool Paga { get; private set; }

    // EF Core
    private DespesaExtra() { }

    public static DespesaExtra Criar(
        Guid usuarioId,
        string descricao,
        decimal valor,
        DateOnly dataDespesa,
        string categoria,
        string formaPagamento,
        DateOnly? pagaEm = null,
        Guid? credorId = null)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));

        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));

        return new DespesaExtra
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            CredorId = credorId,
            Descricao = descricao.Trim(),
            ValorTotal = valor,
            DataDespesa = dataDespesa,
            Categoria = categoria,
            FormaPagamento = formaPagamento,
            PagaEm = pagaEm,
            TipoDespesa = TipoDespesa.Extra,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void MarcarComoPaga(bool paga, DateOnly? dataHoje = null)
    {
        Paga = paga;

        if (!string.Equals(FormaPagamento, FormaPagamentoCartaoCredito, StringComparison.OrdinalIgnoreCase))
            PagaEm = paga ? (PagaEm ?? dataHoje ?? DateOnly.FromDateTime(DateTime.UtcNow)) : null;
    }

    public void Atualizar(string descricao, decimal valor, DateOnly dataDespesa, string categoria, string formaPagamento, DateOnly? pagaEm = null, Guid? credorId = null)
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
        PagaEm = pagaEm;
        CredorId = credorId;
    }
}
