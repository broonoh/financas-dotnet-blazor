using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface ICategoriaDespesaRepository
{
    Task<CategoriaDespesa?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<CategoriaDespesa>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(CategoriaDespesa categoria, CancellationToken ct = default);
    void Atualizar(CategoriaDespesa categoria);
    void Remover(CategoriaDespesa categoria);
}
