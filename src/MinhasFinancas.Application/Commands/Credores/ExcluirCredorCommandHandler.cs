using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Credores;

public class ExcluirCredorCommandHandler : IRequestHandler<ExcluirCredorCommand>
{
    private readonly ICredorRepository _repo;
    private readonly IDespesaRepository _despesaRepo;
    private readonly IUnitOfWork _uow;

    public ExcluirCredorCommandHandler(ICredorRepository repo, IDespesaRepository despesaRepo, IUnitOfWork uow)
    {
        _repo = repo;
        _despesaRepo = despesaRepo;
        _uow = uow;
    }

    public async Task Handle(ExcluirCredorCommand request, CancellationToken cancellationToken)
    {
        var credor = await _repo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Credor não encontrado.");

        if (await _despesaRepo.ExisteDespesaParaCredorAsync(credor.Id, cancellationToken))
            throw new InvalidOperationException("Não é possível excluir um credor com despesas atribuídas.");

        _repo.Remover(credor);
        await _uow.CommitAsync(cancellationToken);
    }
}
