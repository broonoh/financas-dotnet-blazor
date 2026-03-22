using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface IDividaRepository
{
    Task<Divida?> ObterPorIdAsync(Guid id, Guid usuarioId);
    Task<ParcelaDivida?> ObterParcelaPorIdAsync(Guid id);
    Task<IEnumerable<Divida>> ListarPorUsuarioAsync(Guid usuarioId);
    Task AdicionarAsync(Divida divida);
    void Atualizar(Divida divida);
    void Remover(Divida divida);
    void AtualizarParcela(ParcelaDivida parcela);
}
