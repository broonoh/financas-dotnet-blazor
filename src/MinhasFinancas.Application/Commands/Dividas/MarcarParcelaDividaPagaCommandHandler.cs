using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Dividas;

public class MarcarParcelaDividaPagaCommandHandler : IRequestHandler<MarcarParcelaDividaPagaCommand>
{
    private readonly IDividaRepository _dividaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarcarParcelaDividaPagaCommandHandler(IDividaRepository dividaRepository, IUnitOfWork unitOfWork)
    {
        _dividaRepository = dividaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarcarParcelaDividaPagaCommand request, CancellationToken cancellationToken)
    {
        var parcela = await _dividaRepository.ObterParcelaPorIdAsync(request.ParcelaId)
            ?? throw new KeyNotFoundException("Parcela não encontrada.");

        if (request.Paga)
            parcela.MarcarPaga(request.DataPagamento);
        else
            parcela.DesmarcarPaga();

        _dividaRepository.AtualizarParcela(parcela);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
