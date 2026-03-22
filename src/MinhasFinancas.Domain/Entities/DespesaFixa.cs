using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Domain.Entities;

public class DespesaFixa : Despesa
{
    public int QuantidadeParcelas { get; private set; }
    public DateOnly DataCompra { get; private set; }
    public DateOnly DataPrimeiraParcela { get; private set; }
    public FormaPagamentoDespesaFixa FormaPagamento { get; private set; }

    private readonly List<Parcela> _parcelas = new();
    public IReadOnlyList<Parcela> Parcelas => _parcelas.AsReadOnly();

    // EF Core
    private DespesaFixa() { }

    public static DespesaFixa Criar(
        Guid usuarioId,
        string descricao,
        decimal valorTotal,
        int quantidadeParcelas,
        DateOnly dataCompra,
        DateOnly dataPrimeiraParcela,
        string categoria,
        FormaPagamentoDespesaFixa formaPagamento)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));

        if (valorTotal <= 0)
            throw new ArgumentException("Valor total deve ser maior que zero.", nameof(valorTotal));

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
            Descricao = descricao.Trim(),
            ValorTotal = valorTotal,
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

    public void Atualizar(string descricao, string categoria, FormaPagamentoDespesaFixa formaPagamento)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));

        Descricao = descricao.Trim();
        Categoria = categoria;
        FormaPagamento = formaPagamento;
    }

    /// <summary>
    /// Gera parcelas com valor = Math.Floor(total/qtd) e ajuste na última para soma exata.
    /// </summary>
    private void GerarParcelas()
    {
        // Trabalha em centavos para evitar problemas de arredondamento
        var totalCentavos = (long)Math.Round(ValorTotal * 100);
        var parcelaCentavos = totalCentavos / QuantidadeParcelas;
        var restoCentavos = totalCentavos - (parcelaCentavos * QuantidadeParcelas);

        _parcelas.Clear();

        for (int i = 1; i <= QuantidadeParcelas; i++)
        {
            var centavosEstaParcela = parcelaCentavos;
            if (i == QuantidadeParcelas)
                centavosEstaParcela += restoCentavos; // última parcela absorve o resto

            var dataVencimento = DataPrimeiraParcela.AddMonths(i - 1);
            var valorParcela = centavosEstaParcela / 100m;

            _parcelas.Add(Parcela.Criar(Id, i, valorParcela, dataVencimento));
        }
    }
}
