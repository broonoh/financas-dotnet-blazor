using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface IParcelaRepository
{
    Task<Parcela?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Parcela>> ListarPorDespesaAsync(Guid despesaId, CancellationToken ct = default);
    Task<IEnumerable<Parcela>> ListarPorUsuarioMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default);
    Task<decimal> SomarParcelasMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default);
    void Atualizar(Parcela parcela);
}
