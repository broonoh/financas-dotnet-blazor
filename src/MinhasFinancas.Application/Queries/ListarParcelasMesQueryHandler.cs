using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarParcelasMesQueryHandler : IRequestHandler<ListarParcelasMesQuery, IEnumerable<ParcelaDto>>
{
    private readonly IParcelaRepository _parcelaRepo;
    private readonly IDespesaRepository _despesaRepo;

    public ListarParcelasMesQueryHandler(IParcelaRepository parcelaRepo, IDespesaRepository despesaRepo)
    {
        _parcelaRepo = parcelaRepo;
        _despesaRepo = despesaRepo;
    }

    public async Task<IEnumerable<ParcelaDto>> Handle(ListarParcelasMesQuery request, CancellationToken cancellationToken)
    {
        var parcelas = await _parcelaRepo.ListarPorUsuarioMesAsync(request.UsuarioId, request.Ano, request.Mes, cancellationToken);
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var em7Dias = hoje.AddDays(7);

        var result = new List<ParcelaDto>();
        foreach (var p in parcelas)
        {
            var todasParcelas = await _parcelaRepo.ListarPorDespesaAsync(p.DespesaId, cancellationToken);
            var total = todasParcelas.Count();
            var despesa = await _despesaRepo.ObterDespesaFixaPorIdAsync(p.DespesaId, request.UsuarioId, cancellationToken);

            result.Add(new ParcelaDto(
                p.Id,
                p.DespesaId,
                despesa?.Descricao ?? "Despesa",
                p.Numero,
                total,
                p.Valor,
                p.DataVencimento,
                p.Paga,
                p.DataPagamento,
                !p.Paga && p.DataVencimento < hoje,
                !p.Paga && p.DataVencimento >= hoje && p.DataVencimento <= em7Dias));
        }

        return result.OrderBy(p => p.DataVencimento);
    }
}
