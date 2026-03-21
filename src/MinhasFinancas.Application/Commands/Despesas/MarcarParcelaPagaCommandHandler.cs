using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class MarcarParcelaPagaCommandHandler : IRequestHandler<MarcarParcelaPagaCommand>
{
    private readonly IParcelaRepository _parcelaRepo;
    private readonly IUnitOfWork _uow;

    public MarcarParcelaPagaCommandHandler(IParcelaRepository parcelaRepo, IUnitOfWork uow)
    {
        _parcelaRepo = parcelaRepo;
        _uow = uow;
    }

    public async Task Handle(MarcarParcelaPagaCommand request, CancellationToken cancellationToken)
    {
        var parcela = await _parcelaRepo.ObterPorIdAsync(request.ParcelaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Parcela {request.ParcelaId} não encontrada.");

        if (request.Paga)
            parcela.MarcarPaga(request.DataPagamento);
        else
            parcela.DesmarcarPaga();

        _parcelaRepo.Atualizar(parcela);
        await _uow.CommitAsync(cancellationToken);
    }
}
