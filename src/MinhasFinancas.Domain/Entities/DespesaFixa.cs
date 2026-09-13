using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Domain.Entities;

public class DespesaFixa : Despesa
{
    public int QuantidadeParcelas { get; private set; }
    public DateOnly DataCompra { get; private set; }
    public DateOnly DataPrimeiraParcela { get; private set; }
    public string FormaPagamento { get; private set; } = string.Empty;

    private readonly List<Parcela> _parcelas = new();
    public IReadOnlyList<Parcela> Parcelas => _parcelas.AsReadOnly();

    // EF Core
    private DespesaFixa() { }

    public static DespesaFixa Criar(
        Guid usuarioId,
        string descricao,
        decimal valorParcela,
        int quantidadeParcelas,
        DateOnly dataCompra,
        DateOnly dataPrimeiraParcela,
        string categoria,
        string formaPagamento,
        Guid? credorId = null)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));

        if (valorParcela <= 0)
            throw new ArgumentException("Valor da parcela deve ser maior que zero.", nameof(valorParcela));

        if (quantidadeParcelas < 2 || quantidadeParcelas > 48)
            throw new ArgumentException("Quantidade de parcelas deve ser entre 2 e 48.", nameof(quantidadeParcelas));

        var mesAtual = DateOnly.FromDateTime(DateTime.UtcNow);
        var inicioPrimeiraParcela = new DateOnly(dataPrimeiraParcela.Year, dataPrimeiraParcela.Month, 1);
        var inicioMesAtual = new DateOnly(mesAtual.Year, mesAtual.Month, 1);
        if (inicioPrimeiraParcela < inicioMesAtual)
            throw new ArgumentException("Data da primeira parcela não pode ser anterior ao mês atual.", nameof(dataPrimeiraParcela));

        var despesa = new DespesaFixa
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            CredorId = credorId,
            Descricao = descricao.Trim(),
            ValorTotal = valorParcela * quantidadeParcelas,
            QuantidadeParcelas = quantidadeParcelas,
            DataCompra = dataCompra,
            DataPrimeiraParcela = dataPrimeiraParcela,
            Categoria = categoria,
            FormaPagamento = formaPagamento,
            TipoDespesa = TipoDespesa.Fixa,
            DataCriacao = DateTime.UtcNow
        };

        despesa.GerarParcelas();
        return despesa;
    }

    public void Atualizar(string descricao, decimal valorParcela, int quantidadeParcelas, DateOnly dataCompra, DateOnly dataPrimeiraParcela, string categoria, string formaPagamento, Guid? credorId = null)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));
        if (valorParcela <= 0)
            throw new ArgumentException("Valor da parcela deve ser maior que zero.", nameof(valorParcela));
        if (quantidadeParcelas < 2 || quantidadeParcelas > 48)
            throw new ArgumentException("Quantidade de parcelas deve ser entre 2 e 48.", nameof(quantidadeParcelas));

        Descricao = descricao.Trim();
        ValorTotal = valorParcela * quantidadeParcelas;
        QuantidadeParcelas = quantidadeParcelas;
        DataCompra = dataCompra;
        DataPrimeiraParcela = dataPrimeiraParcela;
        Categoria = categoria;
        FormaPagamento = formaPagamento;
        CredorId = credorId;
        GerarParcelas();
    }

    /// <summary>
    /// Gera parcelas de valor fixo e igual (ValorTotal já é o resultado de valorParcela * QuantidadeParcelas).
    /// </summary>
    private void GerarParcelas()
    {
        var valorParcela = ValorTotal / QuantidadeParcelas;

        _parcelas.Clear();

        for (int i = 1; i <= QuantidadeParcelas; i++)
        {
            var dataVencimento = DataPrimeiraParcela.AddMonths(i - 1);
            _parcelas.Add(Parcela.Criar(Id, i, valorParcela, dataVencimento));
        }
    }
}
