using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default)
        => _context.Usuarios.FirstOrDefaultAsync(u => u.Email.Valor == email.ToLowerInvariant(), ct);

    public Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default)
        => _context.Usuarios.AnyAsync(u => u.Email.Valor == email.ToLowerInvariant(), ct);

    public Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
        => _context.Usuarios.AddAsync(usuario, ct).AsTask();

    public void Atualizar(Usuario usuario)
        => _context.Usuarios.Update(usuario);
}
