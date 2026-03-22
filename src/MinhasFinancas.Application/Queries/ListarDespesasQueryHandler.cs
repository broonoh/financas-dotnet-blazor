using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarDespesasFixasQueryHandler : IRequestHandler<ListarDespesasFixasQuery, IEnumerable<DespesaFixaDto>>
{
    private readonly IDespesaRepository _despesaRepository;

    public ListarDespesasFixasQueryHandler(IDespesaRepository despesaRepository)
    {
        _despesaRepository = despesaRepository;
    }

    public async Task<IEnumerable<DespesaFixaDto>> Handle(ListarDespesasFixasQuery request, CancellationToken cancellationToken)
    {
        var despesas = await _despesaRepository.ListarFixasComParcelasAsync(request.UsuarioId, cancellationToken);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        return despesas.Select(d =>
        {
            var parcelas = d.Parcelas
                .OrderBy(p => p.Numero)
                .Select(p => new ParcelaDto(
                    p.Id, p.DespesaId, d.Descricao,
                    p.Numero, d.QuantidadeParcelas,
                    p.Valor, p.DataVencimento,
                    p.Paga, p.DataPagamento,
                    !p.Paga && p.DataVencimento < hoje,
                    !p.Paga && p.DataVencimento >= hoje && p.DataVencimento <= hoje.AddDays(7)))
                .ToList();

            return new DespesaFixaDto(
                d.Id, d.Descricao, d.ValorTotal, d.QuantidadeParcelas,
                d.DataCompra, d.DataPrimeiraParcela, d.Categoria, d.FormaPagamento, d.DataCriacao,
                parcelas);
        });
    }
}

public class ListarDespesasExtrasQueryHandler : IRequestHandler<ListarDespesasExtrasQuery, IEnumerable<DespesaExtraDto>>
{
    private readonly IDespesaRepository _despesaRepository;

    public ListarDespesasExtrasQueryHandler(IDespesaRepository despesaRepository)
    {
        _despesaRepository = despesaRepository;
    }

    public async Task<IEnumerable<DespesaExtraDto>> Handle(ListarDespesasExtrasQuery request, CancellationToken cancellationToken)
    {
        var despesas = await _despesaRepository.ListarExtrasPorUsuarioAsync(request.UsuarioId, cancellationToken);

        return despesas.Select(d => new DespesaExtraDto(
            d.Id, d.Descricao, d.ValorTotal, d.DataDespesa, d.Categoria, d.FormaPagamento, d.DataCriacao));
    }
}
