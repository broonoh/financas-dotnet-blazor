using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Domain.Interfaces;

public interface IFormaPagamentoRepository
{
    Task<FormaPagamento?> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    Task<IEnumerable<FormaPagamento>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
    Task AdicionarAsync(FormaPagamento formaPagamento, CancellationToken ct = default);
    void Atualizar(FormaPagamento formaPagamento);
    void Remover(FormaPagamento formaPagamento);
}
