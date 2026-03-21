namespace MinhasFinancas.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }

    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("Email não pode ser vazio.", nameof(valor));

        var trimmed = valor.Trim().ToLowerInvariant();

        if (!trimmed.Contains('@') || !trimmed.Contains('.'))
            throw new ArgumentException("Email inválido.", nameof(valor));

        Valor = trimmed;
    }

    public static implicit operator string(Email email) => email.Valor;
    public override string ToString() => Valor;
}
