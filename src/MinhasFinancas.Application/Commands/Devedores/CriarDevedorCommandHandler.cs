using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Devedores;

public class CriarDevedorCommandHandler : IRequestHandler<CriarDevedorCommand, DevedorDto>
{
    private readonly IDevedorRepository _repo;
    private readonly IUnitOfWork _uow;

    public CriarDevedorCommandHandler(IDevedorRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<DevedorDto> Handle(CriarDevedorCommand request, CancellationToken cancellationToken)
    {
        var devedor = Devedor.Criar(request.UsuarioId, request.Nome);
        await _repo.AdicionarAsync(devedor, cancellationToken);
        await _uow.CommitAsync(cancellationToken);
        return new DevedorDto(devedor.Id, devedor.Nome, devedor.DataCriacao);
    }
}
