using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class CredorRepository : ICredorRepository
{
    private readonly AppDbContext _context;

    public CredorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Credor?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => await _context.Credores
            .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId, ct);

    public async Task<IEnumerable<Credor>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.Credores
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Credor credor, CancellationToken ct = default)
        => await _context.Credores.AddAsync(credor, ct);

    public void Atualizar(Credor credor)
        => _context.Credores.Update(credor);

    public void Remover(Credor credor)
        => _context.Credores.Remove(credor);
}
