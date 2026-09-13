using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Dividas;

public class AtualizarDividaCommandHandler : IRequestHandler<AtualizarDividaCommand, DividaDto>
{
    private readonly IDividaRepository _dividaRepo;
    private readonly IDevedorRepository _devedorRepo;
    private readonly IUnitOfWork _uow;

    public AtualizarDividaCommandHandler(IDividaRepository dividaRepo, IDevedorRepository devedorRepo, IUnitOfWork uow)
    {
        _dividaRepo = dividaRepo;
        _devedorRepo = devedorRepo;
        _uow = uow;
    }

    public async Task<DividaDto> Handle(AtualizarDividaCommand request, CancellationToken cancellationToken)
    {
        var divida = await _dividaRepo.ObterPorIdAsync(request.Id, request.UsuarioId)
            ?? throw new KeyNotFoundException("Dívida não encontrada.");

        var devedor = await _devedorRepo.ObterPorIdAsync(request.DevedorId, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Devedor não encontrado.");

        divida.Atualizar(request.DevedorId, request.Descricao, request.ValorParcela, request.QuantidadeParcelas, request.DataCompra, request.DataPrimeiraParcela);
        await _dividaRepo.AtualizarAsync(divida, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var parcelas = divida.Parcelas.OrderBy(p => p.Numero).Select(p => new ParcelaDividaDto(
            p.Id, p.DividaId, p.Numero, divida.QuantidadeParcelas,
            p.Valor, p.DataVencimento, p.Paga, p.DataPagamento,
            !p.Paga && p.DataVencimento < hoje)).ToList();

        var saldoRestante = divida.Parcelas.Where(p => !p.Paga).Sum(p => p.Valor);

        return new DividaDto(divida.Id, divida.DevedorId, devedor.Nome, divida.Descricao, divida.ValorTotal,
            saldoRestante, divida.QuantidadeParcelas, divida.DataCompra, divida.DataPrimeiraParcela, divida.Ativa, divida.DataCriacao, parcelas);
    }
}
