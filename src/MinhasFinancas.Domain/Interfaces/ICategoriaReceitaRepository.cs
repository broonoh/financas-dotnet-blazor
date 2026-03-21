using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface ICategoriaReceitaRepository
{
    Task<CategoriaReceita?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<CategoriaReceita>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(CategoriaReceita categoria, CancellationToken ct = default);
    void Atualizar(CategoriaReceita categoria);
    void Remover(CategoriaReceita categoria);
}
