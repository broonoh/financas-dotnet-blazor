namespace MinhasFinancas.Domain.Entities;

public class Divida
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid DevedorId { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal ValorTotal { get; private set; }
    public int QuantidadeParcelas { get; private set; }
    public DateOnly DataCompra { get; private set; }
    public DateOnly DataPrimeiraParcela { get; private set; }
    public bool Ativa { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private readonly List<ParcelaDivida> _parcelas = new();
    public IReadOnlyList<ParcelaDivida> Parcelas => _parcelas.AsReadOnly();

    // EF Core
    private Divida() { }

    public static Divida Criar(
        Guid usuarioId,
        Guid devedorId,
        string descricao,
        decimal valorParcela,
        int quantidadeParcelas,
        DateOnly dataCompra,
        DateOnly dataPrimeiraParcela)
    {
        if (devedorId == Guid.Empty)
            throw new ArgumentException("Devedor é obrigatório.", nameof(devedorId));

        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 200)
            throw new ArgumentException("Descrição deve ter entre 3 e 200 caracteres.", nameof(descricao));

        if (valorParcela <= 0)
            throw new ArgumentException("Valor da parcela deve ser maior que zero.", nameof(valorParcela));

        if (quantidadeParcelas < 1 || quantidadeParcelas > 120)
            throw new ArgumentException("Quantidade de parcelas deve ser entre 1 e 120.", nameof(quantidadeParcelas));

        var divida = new Divida
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            DevedorId = devedorId,
            Descricao = descricao.Trim(),
            ValorTotal = valorParcela * quantidadeParcelas,
            QuantidadeParcelas = quantidadeParcelas,
            DataCompra = dataCompra,
            DataPrimeiraParcela = dataPrimeiraParcela,
            Ativa = true,
            DataCriacao = DateTime.UtcNow
        };

        divida.GerarParcelas();
        return divida;
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
            _parcelas.Add(ParcelaDivida.Criar(Id, i, valorParcela, dataVencimento));
        }
    }

    public void Atualizar(Guid devedorId, string descricao, decimal valorParcela, int quantidadeParcelas, DateOnly dataCompra, DateOnly dataPrimeiraParcela)
    {
        if (devedorId == Guid.Empty)
            throw new ArgumentException("Devedor é obrigatório.", nameof(devedorId));
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 200)
            throw new ArgumentException("Descrição deve ter entre 3 e 200 caracteres.", nameof(descricao));
        if (valorParcela <= 0)
            throw new ArgumentException("Valor da parcela deve ser maior que zero.", nameof(valorParcela));
        if (quantidadeParcelas < 1 || quantidadeParcelas > 120)
            throw new ArgumentException("Quantidade de parcelas deve ser entre 1 e 120.", nameof(quantidadeParcelas));

        DevedorId = devedorId;
        Descricao = descricao.Trim();
        ValorTotal = valorParcela * quantidadeParcelas;
        QuantidadeParcelas = quantidadeParcelas;
        DataCompra = dataCompra;
        DataPrimeiraParcela = dataPrimeiraParcela;
        GerarParcelas();
    }

    public void Encerrar() => Ativa = false;
}
