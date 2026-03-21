namespace MinhasFinancas.Domain.Interfaces;

public interface ISenhaService
{
    string HashSenha(string senha);
    bool VerificarSenha(string senha, string hash);
    bool SenhaForte(string senha);
}
