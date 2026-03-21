using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Dividas;

public class ExcluirDividaCommandHandler : IRequestHandler<ExcluirDividaCommand>
{
    private readonly IDividaRepository _dividaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirDividaCommandHandler(IDividaRepository dividaRepository, IUnitOfWork unitOfWork)
    {
        _dividaRepository = dividaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ExcluirDividaCommand request, CancellationToken cancellationToken)
    {
        var divida = await _dividaRepository.ObterPorIdAsync(request.DividaId, request.UsuarioId)
            ?? throw new KeyNotFoundException("Dívida não encontrada.");

        _dividaRepository.Remover(divida);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
