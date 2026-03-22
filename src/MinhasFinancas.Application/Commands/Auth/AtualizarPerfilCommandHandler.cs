using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;
namespace MinhasFinancas.Application.Commands.Auth;
public class AtualizarPerfilCommandHandler : IRequestHandler<AtualizarPerfilCommand, PerfilDto>
{
    private readonly IUsuarioRepository _repo;
    private readonly IUnitOfWork _uow;
    public AtualizarPerfilCommandHandler(IUsuarioRepository repo, IUnitOfWork uow)
    { _repo = repo; _uow = uow; }
    public async Task<PerfilDto> Handle(AtualizarPerfilCommand request, CancellationToken ct)
    {
        var u = await _repo.ObterPorIdAsync(request.UsuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");
        u.AtualizarPerfil(request.Nome, request.Telefone);
        _repo.Atualizar(u);
        await _uow.CommitAsync(ct);
        return new PerfilDto(u.Id, u.Nome, u.Email.Valor, u.DataNascimento, u.Telefone, u.DataCadastro);
    }
}
