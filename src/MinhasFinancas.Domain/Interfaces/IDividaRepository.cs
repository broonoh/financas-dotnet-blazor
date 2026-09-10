using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface IDividaRepository
{
    Task<Divida?> ObterPorIdAsync(Guid id, Guid usuarioId);
    Task<ParcelaDivida?> ObterParcelaPorIdAsync(Guid id);
    Task<IEnumerable<Divida>> ListarPorUsuarioAsync(Guid usuarioId);
    Task AdicionarAsync(Divida divida);
    Task AtualizarAsync(Divida divida, CancellationToken ct = default);
    void Remover(Divida divida);
    void AtualizarParcela(ParcelaDivida parcela);
    Task<IEnumerable<ParcelaDivida>> ListarParcelasPendentesDoMesAsync(Guid usuarioId, int ano, int mes);
    Task<bool> ExisteDividaParaDevedorAsync(Guid devedorId);
}
