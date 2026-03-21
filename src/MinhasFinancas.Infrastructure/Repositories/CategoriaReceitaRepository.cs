using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class CategoriaReceitaRepository : ICategoriaReceitaRepository
{
    private readonly AppDbContext _context;

    public CategoriaReceitaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoriaReceita?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => await _context.CategoriasReceita
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId, ct);

    public async Task<IEnumerable<CategoriaReceita>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.CategoriasReceita
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(CategoriaReceita categoria, CancellationToken ct = default)
        => await _context.CategoriasReceita.AddAsync(categoria, ct);

    public void Atualizar(CategoriaReceita categoria)
        => _context.CategoriasReceita.Update(categoria);

    public void Remover(CategoriaReceita categoria)
        => _context.CategoriasReceita.Remove(categoria);
}
