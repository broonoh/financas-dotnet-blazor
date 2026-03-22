using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Dividas;

public class AtualizarDividaCommandHandler : IRequestHandler<AtualizarDividaCommand, DividaDto>
{
    private readonly IDividaRepository _dividaRepo;
    private readonly IUnitOfWork _uow;

    public AtualizarDividaCommandHandler(IDividaRepository dividaRepo, IUnitOfWork uow)
    {
        _dividaRepo = dividaRepo;
        _uow = uow;
    }

    public async Task<DividaDto> Handle(AtualizarDividaCommand request, CancellationToken cancellationToken)
    {
        var divida = await _dividaRepo.ObterPorIdAsync(request.Id, request.UsuarioId)
            ?? throw new KeyNotFoundException("Dívida não encontrada.");

        divida.Atualizar(request.NomeDevedor, request.Descricao);
        _dividaRepo.Atualizar(divida);
        await _uow.CommitAsync(cancellationToken);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var parcelas = divida.Parcelas.Select(p => new ParcelaDividaDto(
            p.Id, p.DividaId, p.Numero, divida.QuantidadeParcelas,
            p.Valor, p.DataVencimento, p.Paga, p.DataPagamento,
            !p.Paga && p.DataVencimento < hoje)).ToList();

        var saldoRestante = divida.Parcelas.Where(p => !p.Paga).Sum(p => p.Valor);

        return new DividaDto(divida.Id, divida.NomeDevedor, divida.Descricao, divida.ValorTotal,
            saldoRestante, divida.QuantidadeParcelas, divida.DataCompra, divida.Ativa, divida.DataCriacao, parcelas);
    }
}
