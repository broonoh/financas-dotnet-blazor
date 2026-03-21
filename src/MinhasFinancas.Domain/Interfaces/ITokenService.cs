using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface ITokenService
{
    string GerarAccessToken(Usuario usuario);
    string GerarRefreshToken();
    Guid? ObterUsuarioIdDoToken(string token);
}
