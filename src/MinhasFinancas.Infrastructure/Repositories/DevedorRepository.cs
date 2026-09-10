using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class DevedorRepository : IDevedorRepository
{
    private readonly AppDbContext _context;

    public DevedorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Devedor?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => await _context.Devedores
            .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId, ct);

    public async Task<IEnumerable<Devedor>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.Devedores
            .Where(d => d.UsuarioId == usuarioId)
            .OrderBy(d => d.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Devedor devedor, CancellationToken ct = default)
        => await _context.Devedores.AddAsync(devedor, ct);

    public void Atualizar(Devedor devedor)
        => _context.Devedores.Update(devedor);

    public void Remover(Devedor devedor)
        => _context.Devedores.Remove(devedor);
}
