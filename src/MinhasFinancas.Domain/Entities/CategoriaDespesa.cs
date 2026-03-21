namespace MinhasFinancas.Domain.Entities;

public class CategoriaDespesa
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public DateTime DataCriacao { get; private set; }

    // EF Core
    private CategoriaDespesa() { }

    public static CategoriaDespesa Criar(Guid usuarioId, string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Length < 2 || nome.Length > 50)
            throw new ArgumentException("Nome deve ter entre 2 e 50 caracteres.", nameof(nome));

        return new CategoriaDespesa
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Nome = nome.Trim(),
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Atualizar(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Length < 2 || nome.Length > 50)
            throw new ArgumentException("Nome deve ter entre 2 e 50 caracteres.", nameof(nome));

        Nome = nome.Trim();
    }
}
