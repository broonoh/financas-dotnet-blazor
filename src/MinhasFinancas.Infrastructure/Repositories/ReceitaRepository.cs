using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class ReceitaRepository : IReceitaRepository
{
    private readonly AppDbContext _context;

    public ReceitaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Receita?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => _context.Receitas.FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId, ct);

    public async Task<IEnumerable<Receita>> ListarPorUsuarioMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default)
        => await _context.Receitas
            .Where(r => r.UsuarioId == usuarioId && r.DataRecebimento.Year == ano && r.DataRecebimento.Month == mes)
            .OrderBy(r => r.DataRecebimento)
            .ToListAsync(ct);

    public async Task<IEnumerable<Receita>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.Receitas
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.DataRecebimento)
            .ToListAsync(ct);

    public async Task<decimal[]> ObterTotaisMensaisAsync(Guid usuarioId, int quantidadeMeses, CancellationToken ct = default)
    {
        var inicio = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-quantidadeMeses + 1);
        var receitas = await _context.Receitas
            .Where(r => r.UsuarioId == usuarioId && r.DataRecebimento >= inicio)
            .GroupBy(r => new { r.DataRecebimento.Year, r.DataRecebimento.Month })
            .Select(g => g.Sum(r => r.Valor))
            .ToArrayAsync(ct);
        return receitas;
    }

    public Task AdicionarAsync(Receita receita, CancellationToken ct = default)
        => _context.Receitas.AddAsync(receita, ct).AsTask();

    public Task AdicionarVariasAsync(IEnumerable<Receita> receitas, CancellationToken ct = default)
        => _context.Receitas.AddRangeAsync(receitas, ct);

    public void Atualizar(Receita receita)
        => _context.Receitas.Update(receita);

    public void Remover(Receita receita)
        => _context.Receitas.Remove(receita);
}
