using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class CategoriaDespesaRepository : ICategoriaDespesaRepository
{
    private readonly AppDbContext _context;

    public CategoriaDespesaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoriaDespesa?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => await _context.CategoriasDespesa
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId, ct);

    public async Task<IEnumerable<CategoriaDespesa>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.CategoriasDespesa
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(CategoriaDespesa categoria, CancellationToken ct = default)
        => await _context.CategoriasDespesa.AddAsync(categoria, ct);

    public void Atualizar(CategoriaDespesa categoria)
        => _context.CategoriasDespesa.Update(categoria);

    public void Remover(CategoriaDespesa categoria)
        => _context.CategoriasDespesa.Remove(categoria);
}
