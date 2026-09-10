using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Credores;

public class CriarCredorCommandHandler : IRequestHandler<CriarCredorCommand, CredorDto>
{
    private readonly ICredorRepository _repo;
    private readonly IUnitOfWork _uow;

    public CriarCredorCommandHandler(ICredorRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<CredorDto> Handle(CriarCredorCommand request, CancellationToken cancellationToken)
    {
        var credor = Credor.Criar(request.UsuarioId, request.Nome);
        await _repo.AdicionarAsync(credor, cancellationToken);
        await _uow.CommitAsync(cancellationToken);
        return new CredorDto(credor.Id, credor.Nome, credor.DataCriacao);
    }
}
