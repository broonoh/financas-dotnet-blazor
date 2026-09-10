namespace MinhasFinancas.Domain.Entities;

public class Credor
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public DateTime DataCriacao { get; private set; }

    // EF Core
    private Credor() { }

    public static Credor Criar(Guid usuarioId, string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Length < 2 || nome.Length > 100)
            throw new ArgumentException("Nome do credor deve ter entre 2 e 100 caracteres.", nameof(nome));

        return new Credor
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Nome = nome.Trim(),
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Atualizar(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Length < 2 || nome.Length > 100)
            throw new ArgumentException("Nome do credor deve ter entre 2 e 100 caracteres.", nameof(nome));

        Nome = nome.Trim();
    }
}
