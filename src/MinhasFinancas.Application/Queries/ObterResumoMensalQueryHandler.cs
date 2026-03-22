using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ObterResumoMensalQueryHandler : IRequestHandler<ObterResumoMensalQuery, ResumoMensalDto>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly IDividaRepository _dividaRepo;

    public ObterResumoMensalQueryHandler(IDespesaRepository despesaRepo, IDividaRepository dividaRepo)
    {
        _despesaRepo = despesaRepo;
        _dividaRepo = dividaRepo;
    }

    public async Task<ResumoMensalDto> Handle(ObterResumoMensalQuery request, CancellationToken ct)
    {
        var (usuarioId, ano, mes) = (request.UsuarioId, request.Ano, request.Mes);

        var despesasFixas = await _despesaRepo.ListarFixasComParcelasAsync(usuarioId, ct);
        var despesasExtras = await _despesaRepo.ListarExtrasDoMesAsync(usuarioId, ano, mes, ct);
        var dividas = await _dividaRepo.ListarPorUsuarioAsync(usuarioId);

        // Despesas Fixas — parcelas com vencimento no mês solicitado
        var itensFixas = despesasFixas
            .SelectMany(d => d.Parcelas
                .Where(p => p.DataVencimento.Year == ano && p.DataVencimento.Month == mes)
                .Select(p => new ResumoItemDespesaFixaDto(
                    p.Id, d.Id, d.Descricao, d.Categoria,
                    p.Numero, d.QuantidadeParcelas,
                    p.Valor, p.DataVencimento, p.Paga)))
            .OrderBy(p => p.DataVencimento)
            .ThenBy(p => p.Descricao)
            .ToList();

        // Despesas Extras do mês
        var itensExtras = despesasExtras
            .Select(d => new ResumoItemDespesaExtraDto(
                d.Id, d.Descricao, d.Categoria, d.ValorTotal, d.DataDespesa))
            .OrderBy(d => d.DataDespesa)
            .ThenBy(d => d.Descricao)
            .ToList();

        // Contas a Receber — parcelas por devedor no mês solicitado
        var devedores = dividas
            .Where(d => d.Ativa)
            .Select(d =>
            {
                var parcelasDoMes = d.Parcelas
                    .Where(p => p.DataVencimento.Year == ano && p.DataVencimento.Month == mes)
                    .OrderBy(p => p.Numero)
                    .Select(p => new ResumoParcelaDevedorDto(
                        p.Id, d.Id, d.Descricao,
                        p.Numero, d.QuantidadeParcelas,
                        p.Valor, p.DataVencimento, p.Paga))
                    .ToList();

                return (d.NomeDevedor, Parcelas: parcelasDoMes);
            })
            .Where(x => x.Parcelas.Count > 0)
            .GroupBy(x => x.NomeDevedor)
            .Select(g => new ResumoDevedorDto(
                g.Key,
                g.SelectMany(x => x.Parcelas).OrderBy(p => p.DataVencimento).ToList(),
                g.SelectMany(x => x.Parcelas).Sum(p => p.Valor)))
            .OrderBy(d => d.NomeDevedor)
            .ToList();

        return new ResumoMensalDto(
            ano, mes,
            itensFixas, itensExtras, devedores,
            itensFixas.Sum(x => x.Valor),
            itensExtras.Sum(x => x.Valor),
            devedores.Sum(x => x.Total));
    }
}
