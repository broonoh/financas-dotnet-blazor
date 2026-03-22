using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface IDespesaRepository
{
    Task<DespesaFixa?> ObterDespesaFixaPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<DespesaExtra?> ObterDespesaExtraPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<DespesaFixa>> ListarFixasPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<DespesaFixa>> ListarFixasComParcelasAsync(Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<DespesaExtra>> ListarExtrasPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<DespesaExtra>> ListarExtrasDoMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default);
    Task<decimal[]> ObterTotaisDesepasaMensaisAsync(Guid usuarioId, int quantidadeMeses, CancellationToken ct = default);
    Task AdicionarFixaAsync(DespesaFixa despesa, CancellationToken ct = default);
    Task AdicionarExtraAsync(DespesaExtra despesa, CancellationToken ct = default);
    void AtualizarFixa(DespesaFixa despesa);
    void AtualizarExtra(DespesaExtra despesa);
    void Remover(Despesa despesa);
}
