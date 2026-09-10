using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Dividas;

public class MarcarTodasParcelasDividaPagasDoMesCommandHandler : IRequestHandler<MarcarTodasParcelasDividaPagasDoMesCommand, int>
{
    private readonly IDividaRepository _dividaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarcarTodasParcelasDividaPagasDoMesCommandHandler(IDividaRepository dividaRepository, IUnitOfWork unitOfWork)
    {
        _dividaRepository = dividaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(MarcarTodasParcelasDividaPagasDoMesCommand request, CancellationToken cancellationToken)
    {
        var parcelas = (await _dividaRepository.ListarParcelasPendentesDoMesAsync(request.UsuarioId, request.Ano, request.Mes)).ToList();

        foreach (var parcela in parcelas)
        {
            parcela.MarcarPaga();
            _dividaRepository.AtualizarParcela(parcela);
        }

        if (parcelas.Count > 0)
            await _unitOfWork.CommitAsync(cancellationToken);

        return parcelas.Count;
    }
}
