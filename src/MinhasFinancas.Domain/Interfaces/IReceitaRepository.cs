using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface IReceitaRepository
{
    Task<Receita?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<Receita>> ListarPorUsuarioMesAsync(Guid usuarioId, int ano, int mes, CancellationToken ct = default);
    Task<IEnumerable<Receita>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task<decimal[]> ObterTotaisMensaisAsync(Guid usuarioId, int quantidadeMeses, CancellationToken ct = default);
    Task AdicionarAsync(Receita receita, CancellationToken ct = default);
    Task AdicionarVariasAsync(IEnumerable<Receita> receitas, CancellationToken ct = default);
    void Atualizar(Receita receita);
    void Remover(Receita receita);
}
