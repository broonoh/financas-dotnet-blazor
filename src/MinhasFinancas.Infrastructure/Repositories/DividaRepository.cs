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

    public async Task AtualizarAsync(Divida divida, CancellationToken ct = default)
    {
        // Carrega as parcelas antigas direto do banco (evita conflitos com o
        // cache .Local após GerarParcelas() ter feito _parcelas.Clear())
        var antigas = await _context.ParcelasDivida
            .Where(p => p.DividaId == divida.Id)
            .ToListAsync(ct);
        _context.ParcelasDivida.RemoveRange(antigas);

        // Novas parcelas geradas por GerarParcelas() — todas Detached
        foreach (var p in divida.Parcelas)
            _context.ParcelasDivida.Add(p);

        // Marca a dívida como modificada para gerar o UPDATE
        _context.Entry(divida).State = EntityState.Modified;
    }

    public void Remover(Divida divida)
        => _context.Dividas.Remove(divida);

    public void AtualizarParcela(ParcelaDivida parcela)
        => _context.ParcelasDivida.Update(parcela);

    public async Task<IEnumerable<ParcelaDivida>> ListarParcelasPendentesDoMesAsync(Guid usuarioId, int ano, int mes)
        => await _context.ParcelasDivida
            .Join(_context.Dividas, p => p.DividaId, d => d.Id, (p, d) => new { Parcela = p, d.UsuarioId })
            .Where(x => x.UsuarioId == usuarioId
                && !x.Parcela.Paga
                && x.Parcela.DataVencimento.Year == ano
                && x.Parcela.DataVencimento.Month == mes)
            .Select(x => x.Parcela)
            .ToListAsync();

    public Task<bool> ExisteDividaParaDevedorAsync(Guid devedorId)
        => _context.Dividas.AnyAsync(d => d.DevedorId == devedorId);
}
