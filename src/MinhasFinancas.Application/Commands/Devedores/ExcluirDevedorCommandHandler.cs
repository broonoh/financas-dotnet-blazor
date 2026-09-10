using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Devedores;

public class ExcluirDevedorCommandHandler : IRequestHandler<ExcluirDevedorCommand>
{
    private readonly IDevedorRepository _repo;
    private readonly IDividaRepository _dividaRepo;
    private readonly IUnitOfWork _uow;

    public ExcluirDevedorCommandHandler(IDevedorRepository repo, IDividaRepository dividaRepo, IUnitOfWork uow)
    {
        _repo = repo;
        _dividaRepo = dividaRepo;
        _uow = uow;
    }

    public async Task Handle(ExcluirDevedorCommand request, CancellationToken cancellationToken)
    {
        var devedor = await _repo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Devedor não encontrado.");

        if (await _dividaRepo.ExisteDividaParaDevedorAsync(devedor.Id))
            throw new InvalidOperationException("Não é possível excluir um devedor com dívidas cadastradas.");

        _repo.Remover(devedor);
        await _uow.CommitAsync(cancellationToken);
    }
}
