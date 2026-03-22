using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;
using MinhasFinancas.Infrastructure.Data;

namespace MinhasFinancas.Infrastructure.Repositories;

public class DividaRepository : IDividaRepository
{
    private readonly AppDbContext _context;

    public DividaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Divida?> ObterPorIdAsync(Guid id, Guid usuarioId)
        => _context.Dividas
            .Include(d => d.Parcelas)
            .FirstOrDefaultAsync(d => d.Id == id && d.UsuarioId == usuarioId);

    public Task<ParcelaDivida?> ObterParcelaPorIdAsync(Guid id)
        => _context.ParcelasDivida.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Divida>> ListarPorUsuarioAsync(Guid usuarioId)
        => await _context.Dividas
            .Include(d => d.Parcelas)
            .Where(d => d.UsuarioId == usuarioId)
            .OrderByDescending(d => d.DataCriacao)
            .ToListAsync();

    public Task AdicionarAsync(Divida divida)
        => _context.Dividas.AddAsync(divida).AsTask();

    public void Atualizar(Divida divida)
        => _context.Dividas.Update(divida);

    public void Remover(Divida divida)
        => _context.Dividas.Remove(divida);

    public void AtualizarParcela(ParcelaDivida parcela)
        => _context.ParcelasDivida.Update(parcela);
}
