using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Infrastructure.Services;

/// <summary>
/// Implementa hash de senha usando BCrypt com custo 12 (recomendado para segurança atual).
/// </summary>
public class SenhaService : ISenhaService
{
    private const int WorkFactor = 12;

    public string HashSenha(string senha)
        => BCrypt.Net.BCrypt.HashPassword(senha, WorkFactor);

    public bool VerificarSenha(string senha, string hash)
        => BCrypt.Net.BCrypt.Verify(senha, hash);

    public bool SenhaForte(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 12) return false;
        bool temMaiuscula = senha.Any(char.IsUpper);
        bool temMinuscula = senha.Any(char.IsLower);
        bool temNumero = senha.Any(char.IsDigit);
        bool temEspecial = senha.Any(c => !char.IsLetterOrDigit(c));
        return temMaiuscula && temMinuscula && temNumero && temEspecial;
    }
}
