namespace MinhasFinancas.Domain.ValueObjects;

public record SenhaHash
{
    public string Valor { get; }

    public SenhaHash(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Hash de senha não pode ser vazio.", nameof(valor));

        Valor = valor;
    }

    public static implicit operator string(SenhaHash hash) => hash.Valor;
    public override string ToString() => Valor;
}
