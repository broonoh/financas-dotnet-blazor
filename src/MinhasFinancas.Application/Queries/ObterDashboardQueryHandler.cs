using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ObterDashboardQueryHandler : IRequestHandler<ObterDashboardQuery, DashboardDto>
{
    private readonly IReceitaRepository _receitaRepo;
    private readonly IParcelaRepository _parcelaRepo;
    private readonly IDespesaRepository _despesaRepo;

    public ObterDashboardQueryHandler(
        IReceitaRepository receitaRepo,
        IParcelaRepository parcelaRepo,
        IDespesaRepository despesaRepo)
    {
        _receitaRepo = receitaRepo;
        _parcelaRepo = parcelaRepo;
        _despesaRepo = despesaRepo;
    }

    public async Task<DashboardDto> Handle(ObterDashboardQuery request, CancellationToken cancellationToken)
    {
        var totalReceitas = (await _receitaRepo.ListarPorUsuarioMesAsync(request.UsuarioId, request.Ano, request.Mes, cancellationToken))
            .Sum(r => r.Valor);

        var totalParcelas = await _parcelaRepo.SomarParcelasMesAsync(request.UsuarioId, request.Ano, request.Mes, cancellationToken);
        var despesasExtra = await _despesaRepo.ListarExtrasDoMesAsync(request.UsuarioId, request.Ano, request.Mes, cancellationToken);
        var totalExtras = despesasExtra.Sum(d => d.ValorTotal);
        var totalDespesas = totalParcelas + totalExtras;

        var saldo = totalReceitas - totalDespesas;

        // Evolução 12 meses (últimos 12 meses)
        var evolucao = new List<DadosMensaisDto>();
        for (int i = 11; i >= 0; i--)
        {
            var data = new DateOnly(request.Ano, request.Mes, 1).AddMonths(-i);
            var receitasMes = (await _receitaRepo.ListarPorUsuarioMesAsync(request.UsuarioId, data.Year, data.Month, cancellationToken))
                .Sum(r => r.Valor);
            var parcelasMes = await _parcelaRepo.SomarParcelasMesAsync(request.UsuarioId, data.Year, data.Month, cancellationToken);
            var extrasMes = (await _despesaRepo.ListarExtrasDoMesAsync(request.UsuarioId, data.Year, data.Month, cancellationToken))
                .Sum(d => d.ValorTotal);
            var despesasMes = parcelasMes + extrasMes;

            evolucao.Add(new DadosMensaisDto(
                data.ToString("MMM/yy"),
                receitasMes,
                despesasMes,
                receitasMes - despesasMes));
        }

        // Distribuição por categoria do mês atual
        var parcelasMesAtual = await _parcelaRepo.ListarPorUsuarioMesAsync(request.UsuarioId, request.Ano, request.Mes, cancellationToken);
        var distribuicao = parcelasMesAtual
            .GroupBy(p => "Fixas")
            .Select(g => new CategoriaValorDto(g.Key, g.Sum(p => p.Valor), totalDespesas > 0 ? g.Sum(p => p.Valor) / totalDespesas * 100 : 0))
            .ToList();

        if (totalExtras > 0)
            distribuicao.Add(new CategoriaValorDto("Extras", totalExtras, totalDespesas > 0 ? totalExtras / totalDespesas * 100 : 0));

        // Comparativo 6 meses
        var comparativo = evolucao.TakeLast(6).ToList();

        // Projeção: média de despesas dos últimos 3 meses
        var projecao = evolucao.TakeLast(3).Average(e => e.Despesas);

        return new DashboardDto(saldo, totalReceitas, totalDespesas, projecao, evolucao, distribuicao, comparativo);
    }
}
