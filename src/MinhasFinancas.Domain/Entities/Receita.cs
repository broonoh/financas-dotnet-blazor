namespace MinhasFinancas.Domain.Entities;

public class Receita
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly DataRecebimento { get; private set; }
    public string Categoria { get; private set; } = string.Empty;
    public bool Recorrente { get; private set; }
    public DateTime DataCriacao { get; private set; }

    // EF Core
    private Receita() { }

    public static Receita Criar(
        Guid usuarioId,
        string descricao,
        decimal valor,
        DateOnly dataRecebimento,
        string categoria,
        bool recorrente = false)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));

        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));

        return new Receita
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Descricao = descricao.Trim(),
            Valor = valor,
            DataRecebimento = dataRecebimento,
            Categoria = categoria,
            Recorrente = recorrente,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Atualizar(string descricao, decimal valor, DateOnly dataRecebimento, string categoria)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));

        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));

        Descricao = descricao.Trim();
        Valor = valor;
        DataRecebimento = dataRecebimento;
        Categoria = categoria;
    }
}
