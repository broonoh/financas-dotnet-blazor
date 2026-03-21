using MinhasFinancas.Domain.ValueObjects;

namespace MinhasFinancas.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public SenhaHash SenhaHash { get; private set; } = null!;
    public DateOnly DataNascimento { get; private set; }
    public string? Telefone { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public bool Ativo { get; private set; }

    // EF Core
    private Usuario() { }

    public static Usuario Criar(
        string nome,
        string email,
        string senhaHash,
        DateOnly dataNascimento,
        string? telefone = null)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Length < 3 || nome.Length > 100)
            throw new ArgumentException("Nome deve ter entre 3 e 100 caracteres.", nameof(nome));

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var idade = hoje.Year - dataNascimento.Year;
        if (dataNascimento > hoje.AddYears(-idade)) idade--;
        if (idade < 18)
            throw new ArgumentException("Usuário deve ter pelo menos 18 anos.", nameof(dataNascimento));

        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            Email = new Email(email),
            SenhaHash = new SenhaHash(senhaHash),
            DataNascimento = dataNascimento,
            Telefone = telefone?.Trim(),
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };
    }

    public void AtualizarRefreshToken(string? token, DateTime? expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public bool RefreshTokenValido(string token)
        => RefreshToken == token && RefreshTokenExpiry > DateTime.UtcNow;

    public void Desativar() => Ativo = false;
}
