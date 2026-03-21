using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class ParcelaRepository : IParcelaRepository
{
    private readonly AppDbContext _context;

    public ParcelaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Parcela?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => _context.Parcelas.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IEnumerable<Parcela>> ListarPorDespesaAsync(Guid despesaId, CancellationToken ct = default)
        => await _context.Parcelas
            .Where(p => p.DespesaId == despesaId)
            .OrderBy(p => p.Numero)
            .ToListAsync(ct);

    public async Task<IEnumerable<Parcela>> ListarPorUsuarioMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default)
        => await _context.Parcelas
            .Where(p => p.DataVencimento.Year == ano
                     && p.DataVencimento.Month == mes
                     && _context.Despesas.Any(d => d.Id == p.DespesaId && d.UsuarioId == usuarioId))
            .OrderBy(p => p.DataVencimento)
            .ToListAsync(ct);

    public Task<decimal> SomarParcelasMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default)
        => _context.Parcelas
            .Where(p => p.DataVencimento.Year == ano
                     && p.DataVencimento.Month == mes
                     && _context.Despesas.Any(d => d.Id == p.DespesaId && d.UsuarioId == usuarioId))
            .SumAsync(p => p.Valor, ct);

    public void Atualizar(Parcela parcela)
        => _context.Parcelas.Update(parcela);
}
