namespace MinhasFinancas.Domain.Entities;

public class Parcela
{
    public Guid Id { get; private set; }
    public Guid DespesaId { get; private set; }
    public int Numero { get; private set; }
    public decimal Valor { get; private set; }
    public DateOnly DataVencimento { get; private set; }
    public bool Paga { get; private set; }
    public DateOnly? DataPagamento { get; private set; }

    // EF Core
    private Parcela() { }

    public static Parcela Criar(Guid despesaId, int numero, decimal valor, DateOnly dataVencimento)
    {
        if (valor <= 0)
            throw new ArgumentException("Valor da parcela deve ser maior que zero.", nameof(valor));

        if (numero < 1)
            throw new ArgumentException("Número da parcela deve ser maior que zero.", nameof(numero));

        return new Parcela
        {
            Id = Guid.NewGuid(),
            DespesaId = despesaId,
            Numero = numero,
            Valor = valor,
            DataVencimento = dataVencimento,
            Paga = false
        };
    }

    public void MarcarPaga(DateOnly? dataPagamento = null)
    {
        Paga = true;
        DataPagamento = dataPagamento ?? DateOnly.FromDateTime(DateTime.UtcNow);
    }

    public void DesmarcarPaga()
    {
        Paga = false;
        DataPagamento = null;
    }

    public void AtualizarValor(decimal novoValor)
    {
        if (novoValor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(novoValor));
        Valor = novoValor;
    }

    public void AtualizarDataVencimento(DateOnly novaData) => DataVencimento = novaData;
}
