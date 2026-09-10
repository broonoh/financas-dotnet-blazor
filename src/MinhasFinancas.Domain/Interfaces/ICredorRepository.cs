using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface ICredorRepository
{
    Task<Credor?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<Credor>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(Credor credor, CancellationToken ct = default);
    void Atualizar(Credor credor);
    void Remover(Credor credor);
}
