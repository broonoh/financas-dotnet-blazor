using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Domain.Entities;

public abstract class Despesa
{
    public Guid Id { get; protected set; }
    public Guid UsuarioId { get; protected set; }
    public string Descricao { get; protected set; } = string.Empty;
    public decimal ValorTotal { get; protected set; }
    public CategoriaDespesa Categoria { get; protected set; }
    public TipoDespesa TipoDespesa { get; protected set; }
    public DateTime DataCriacao { get; protected set; }
}
