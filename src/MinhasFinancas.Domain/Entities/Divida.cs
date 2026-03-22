namespace MinhasFinancas.Domain.Entities;

public class Divida
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string NomeDevedor { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal ValorTotal { get; private set; }
    public int QuantidadeParcelas { get; private set; }
    public DateOnly DataCompra { get; private set; }
    public bool Ativa { get; private set; }
    public DateTime DataCriacao { get; private set; }

    private readonly List<ParcelaDivida> _parcelas = new();
    public IReadOnlyList<ParcelaDivida> Parcelas => _parcelas.AsReadOnly();

    // EF Core
    private Divida() { }

    public static Divida Criar(
        Guid usuarioId,
        string nomeDevedor,
        string descricao,
        decimal valorTotal,
        int quantidadeParcelas,
        DateOnly dataCompra)
    {
        if (string.IsNullOrWhiteSpace(nomeDevedor) || nomeDevedor.Length < 2 || nomeDevedor.Length > 100)
            throw new ArgumentException("Nome do devedor deve ter entre 2 e 100 caracteres.", nameof(nomeDevedor));

        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 200)
            throw new ArgumentException("Descrição deve ter entre 3 e 200 caracteres.", nameof(descricao));

        if (valorTotal <= 0)
            throw new ArgumentException("Valor total deve ser maior que zero.", nameof(valorTotal));

        if (quantidadeParcelas < 1 || quantidadeParcelas > 120)
            throw new ArgumentException("Quantidade de parcelas deve ser entre 1 e 120.", nameof(quantidadeParcelas));

        var divida = new Divida
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            NomeDevedor = nomeDevedor.Trim(),
            Descricao = descricao.Trim(),
            ValorTotal = valorTotal,
            QuantidadeParcelas = quantidadeParcelas,
            DataCompra = dataCompra,
            Ativa = true,
            DataCriacao = DateTime.UtcNow
        };

        divida.GerarParcelas();
        return divida;
    }

    private void GerarParcelas()
    {
        var totalCentavos = (long)Math.Round(ValorTotal * 100);
        var parcelaCentavos = totalCentavos / QuantidadeParcelas;
        var restoCentavos = totalCentavos - (parcelaCentavos * QuantidadeParcelas);

        _parcelas.Clear();

        for (int i = 1; i <= QuantidadeParcelas; i++)
        {
            var centavosEstaParcela = parcelaCentavos;
            if (i == QuantidadeParcelas)
                centavosEstaParcela += restoCentavos;

            var dataVencimento = DataCompra.AddMonths(i - 1);
            var valorParcela = centavosEstaParcela / 100m;

            _parcelas.Add(ParcelaDivida.Criar(Id, i, valorParcela, dataVencimento));
        }
    }

    public void Atualizar(string nomeDevedor, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nomeDevedor) || nomeDevedor.Length < 2 || nomeDevedor.Length > 100)
            throw new ArgumentException("Nome do devedor deve ter entre 2 e 100 caracteres.", nameof(nomeDevedor));
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 200)
            throw new ArgumentException("Descrição deve ter entre 3 e 200 caracteres.", nameof(descricao));

        NomeDevedor = nomeDevedor.Trim();
        Descricao = descricao.Trim();
    }

    public void Encerrar() => Ativa = false;
}
