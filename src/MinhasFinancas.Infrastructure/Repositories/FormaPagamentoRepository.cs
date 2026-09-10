using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class FormaPagamentoRepository : IFormaPagamentoRepository
{
    private readonly AppDbContext _context;

    public FormaPagamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FormaPagamento?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        => await _context.FormasPagamento
            .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId == usuarioId, ct);

    public async Task<IEnumerable<FormaPagamento>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
        => await _context.FormasPagamento
            .Where(f => f.UsuarioId == usuarioId)
            .OrderBy(f => f.Nome)
            .ToListAsync(ct);

    public async Task AdicionarAsync(FormaPagamento formaPagamento, CancellationToken ct = default)
        => await _context.FormasPagamento.AddAsync(formaPagamento, ct);

    public void Atualizar(FormaPagamento formaPagamento)
        => _context.FormasPagamento.Update(formaPagamento);

    public void Remover(FormaPagamento formaPagamento)
        => _context.FormasPagamento.Remove(formaPagamento);
}
