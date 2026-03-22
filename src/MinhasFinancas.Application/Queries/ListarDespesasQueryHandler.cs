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
        var despesas = await _despesaRepository.ListarFixasPorUsuarioAsync(request.UsuarioId, cancellationToken);

        return despesas.Select(d => new DespesaFixaDto(
            d.Id, d.Descricao, d.ValorTotal, d.QuantidadeParcelas,
            d.DataCompra, d.DataPrimeiraParcela, d.Categoria, d.FormaPagamento, d.DataCriacao));
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
