using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarDividasQueryHandler : IRequestHandler<ListarDividasQuery, IEnumerable<DividaDto>>
{
    private readonly IDividaRepository _dividaRepository;

    public ListarDividasQueryHandler(IDividaRepository dividaRepository)
    {
        _dividaRepository = dividaRepository;
    }

    public async Task<IEnumerable<DividaDto>> Handle(ListarDividasQuery request, CancellationToken cancellationToken)
    {
        var dividas = await _dividaRepository.ListarPorUsuarioAsync(request.UsuarioId);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        return dividas.Select(d => MapToDto(d, hoje)).ToList();
    }

    private static DividaDto MapToDto(Divida divida, DateOnly hoje)
    {
        var parcelas = divida.Parcelas.Select(p => new ParcelaDividaDto(
            p.Id, p.DividaId, p.Numero, divida.QuantidadeParcelas,
            p.Valor, p.DataVencimento, p.Paga, p.DataPagamento,
            !p.Paga && p.DataVencimento < hoje)).ToList();

        var saldoRestante = divida.Parcelas.Where(p => !p.Paga).Sum(p => p.Valor);

        return new DividaDto(
            divida.Id, divida.NomeDevedor, divida.Descricao, divida.ValorTotal,
            saldoRestante, divida.QuantidadeParcelas, divida.DataCompra,
            divida.Ativa, divida.DataCriacao, parcelas);
    }
}
