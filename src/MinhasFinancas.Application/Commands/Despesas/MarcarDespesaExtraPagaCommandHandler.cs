using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class MarcarDespesaExtraPagaCommandHandler : IRequestHandler<MarcarDespesaExtraPagaCommand>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly IUnitOfWork _uow;

    public MarcarDespesaExtraPagaCommandHandler(IDespesaRepository despesaRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _uow = uow;
    }

    public async Task Handle(MarcarDespesaExtraPagaCommand request, CancellationToken cancellationToken)
    {
        var despesa = await _despesaRepo.ObterDespesaExtraPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Despesa extra não encontrada.");

        despesa.MarcarComoPaga(request.Paga, DateOnly.FromDateTime(DateTime.UtcNow));
        _despesaRepo.AtualizarExtra(despesa);
        await _uow.CommitAsync(cancellationToken);
    }
}