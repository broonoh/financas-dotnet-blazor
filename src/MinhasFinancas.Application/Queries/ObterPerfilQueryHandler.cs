using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;
namespace MinhasFinancas.Application.Queries;
public class ObterPerfilQueryHandler : IRequestHandler<ObterPerfilQuery, PerfilDto>
{
    private readonly IUsuarioRepository _repo;
    public ObterPerfilQueryHandler(IUsuarioRepository repo) => _repo = repo;
    public async Task<PerfilDto> Handle(ObterPerfilQuery request, CancellationToken ct)
    {
        var u = await _repo.ObterPorIdAsync(request.UsuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");
        return new PerfilDto(u.Id, u.Nome, u.Email.Valor, u.DataNascimento, u.Telefone, u.DataCadastro);
    }
}
