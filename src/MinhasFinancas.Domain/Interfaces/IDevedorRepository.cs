using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface IDevedorRepository
{
    Task<Devedor?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<Devedor>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Devedor devedor, CancellationToken ct = default);
    void Atualizar(Devedor devedor);
    void Remover(Devedor devedor);
}
