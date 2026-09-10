using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarDespesasFixasQueryHandler : IRequestHandler<ListarDespesasFixasQuery, IEnumerable<DespesaFixaDto>>
{
    private readonly IDespesaRepository _despesaRepository;
    private readonly ICredorRepository _credorRepository;

    public ListarDespesasFixasQueryHandler(IDespesaRepository despesaRepository, ICredorRepository credorRepository)
    {
        _despesaRepository = despesaRepository;
        _credorRepository = credorRepository;
    }

    public async Task<IEnumerable<DespesaFixaDto>> Handle(ListarDespesasFixasQuery request, CancellationToken cancellationToken)
    {
        var despesas = await _despesaRepository.ListarFixasComParcelasAsync(request.UsuarioId, cancellationToken);
        var credores = await _credorRepository.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken);
        var nomesPorId = credores.ToDictionary(c => c.Id, c => c.Nome);
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

            var nomeCredor = d.CredorId is Guid credorId ? nomesPorId.GetValueOrDefault(credorId) : null;

            return new DespesaFixaDto(
                d.Id, d.Descricao, d.ValorTotal, d.QuantidadeParcelas,
                d.DataCompra, d.DataPrimeiraParcela, d.Categoria, d.FormaPagamento, d.DataCriacao,
                parcelas, d.CredorId, nomeCredor);
        });
    }
}

public class ListarDespesasExtrasQueryHandler : IRequestHandler<ListarDespesasExtrasQuery, IEnumerable<DespesaExtraDto>>
{
    private readonly IDespesaRepository _despesaRepository;
    private readonly ICredorRepository _credorRepository;

    public ListarDespesasExtrasQueryHandler(IDespesaRepository despesaRepository, ICredorRepository credorRepository)
    {
        _despesaRepository = despesaRepository;
        _credorRepository = credorRepository;
    }

    public async Task<IEnumerable<DespesaExtraDto>> Handle(ListarDespesasExtrasQuery request, CancellationToken cancellationToken)
    {
        var despesas = await _despesaRepository.ListarExtrasPorUsuarioAsync(request.UsuarioId, cancellationToken);
        var credores = await _credorRepository.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken);
        var nomesPorId = credores.ToDictionary(c => c.Id, c => c.Nome);

        return despesas.Select(d => new DespesaExtraDto(
            d.Id, d.Descricao, d.ValorTotal, d.DataDespesa, d.Categoria, d.FormaPagamento, d.PagaEm, d.Paga, d.DataCriacao,
            d.CredorId, d.CredorId is Guid credorId ? nomesPorId.GetValueOrDefault(credorId) : null));
    }
}
