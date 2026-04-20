using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class DespesaRepository : IDespesaRepository
{
    private readonly AppDbContext _context;

    public DespesaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<DespesaFixa?> ObterDespesaFixaPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => _context.DespesasFixas
            .Include(d => d.Parcelas)
            .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId, ct);

    public Task<DespesaExtra?> ObterDespesaExtraPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => _context.DespesasExtras.FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId, ct);

    public async Task<IEnumerable<DespesaFixa>> ListarFixasPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.DespesasFixas
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.DataCriacao)
            .ToListAsync(ct);

    public async Task<IEnumerable<DespesaFixa>> ListarFixasComParcelasAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.DespesasFixas
            .Include(d => d.Parcelas)
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.DataCriacao)
            .ToListAsync(ct);

    public async Task<IEnumerable<DespesaExtra>> ListarExtrasPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.DespesasExtras
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.DataDespesa)
            .ToListAsync(ct);

    public async Task<IEnumerable<DespesaExtra>> ListarExtrasDoMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default)
        => await _context.DespesasExtras
            .Where(d => d.UsuarioId == usuarioId &&
                (d.PagaEm != null
                    ? d.PagaEm.Value.Year == ano && d.PagaEm.Value.Month == mes
                    : d.DataDespesa.Year == ano && d.DataDespesa.Month == mes))
            .ToListAsync(ct);

    public async Task<decimal[]> ObterTotaisDesepasaMensaisAsync(Guid usuarioId, int quantidadeMeses, CancellationToken ct = default)
    {
        var inicio = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-quantidadeMeses + 1);
        var extras = await _context.DespesasExtras
            .Where(d => d.UsuarioId == usuarioId && d.DataDespesa >= inicio)
            .GroupBy(d => new { d.DataDespesa.Year, d.DataDespesa.Month })
            .Select(g => g.Sum(d => d.ValorTotal))
            .ToArrayAsync(ct);
        return extras;
    }

    public Task AdicionarFixaAsync(DespesaFixa despesa, CancellationToken ct = default)
        => _context.DespesasFixas.AddAsync(despesa, ct).AsTask();

    public Task AdicionarExtraAsync(DespesaExtra despesa, CancellationToken ct = default)
        => _context.DespesasExtras.AddAsync(despesa, ct).AsTask();

    public async Task AtualizarFixaAsync(DespesaFixa despesa, CancellationToken ct = default)
    {
        // Carrega as parcelas antigas direto do banco (evita conflitos com o
        // cache .Local após GerarParcelas() ter feito _parcelas.Clear())
        var antigas = await _context.Parcelas
            .Where(p => p.DespesaId == despesa.Id)
            .ToListAsync(ct);
        _context.Parcelas.RemoveRange(antigas);

        // Novas parcelas geradas por GerarParcelas() — todas Detached
        foreach (var p in despesa.Parcelas)
            _context.Parcelas.Add(p);

        // Marca a despesa como modificada para gerar o UPDATE
        _context.Entry(despesa).State = EntityState.Modified;
    }

    public void AtualizarExtra(DespesaExtra despesa)
        => _context.DespesasExtras.Update(despesa);

    public void Remover(Despesa despesa)
        => _context.Despesas.Remove(despesa);
}
