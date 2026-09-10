using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Credores;

public class AtualizarCredorCommandHandler : IRequestHandler<AtualizarCredorCommand, CredorDto>
{
    private readonly ICredorRepository _repo;
    private readonly IUnitOfWork _uow;

    public AtualizarCredorCommandHandler(ICredorRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<CredorDto> Handle(AtualizarCredorCommand request, CancellationToken cancellationToken)
    {
        var credor = await _repo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Credor não encontrado.");

        credor.Atualizar(request.Nome);
        _repo.Atualizar(credor);
        await _uow.CommitAsync(cancellationToken);

        return new CredorDto(credor.Id, credor.Nome, credor.DataCriacao);
    }
}
