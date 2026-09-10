using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Devedores;

public class AtualizarDevedorCommandHandler : IRequestHandler<AtualizarDevedorCommand, DevedorDto>
{
    private readonly IDevedorRepository _repo;
    private readonly IUnitOfWork _uow;

    public AtualizarDevedorCommandHandler(IDevedorRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<DevedorDto> Handle(AtualizarDevedorCommand request, CancellationToken cancellationToken)
    {
        var devedor = await _repo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Devedor não encontrado.");

        devedor.Atualizar(request.Nome);
        _repo.Atualizar(devedor);
        await _uow.CommitAsync(cancellationToken);

        return new DevedorDto(devedor.Id, devedor.Nome, devedor.DataCriacao);
    }
}
