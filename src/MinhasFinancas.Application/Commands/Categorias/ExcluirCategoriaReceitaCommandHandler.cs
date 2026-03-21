using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Categorias;

public class ExcluirCategoriaReceitaCommandHandler : IRequestHandler<ExcluirCategoriaReceitaCommand>
{
    private readonly ICategoriaReceitaRepository _repo;
    private readonly IUnitOfWork _uow;

    public ExcluirCategoriaReceitaCommandHandler(ICategoriaReceitaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task Handle(ExcluirCategoriaReceitaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _repo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Categoria não encontrada.");

        _repo.Remover(categoria);
        await _uow.CommitAsync(cancellationToken);
    }
}
