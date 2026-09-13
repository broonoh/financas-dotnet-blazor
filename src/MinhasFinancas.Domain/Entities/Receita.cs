namespace MinhasFinancas.Domain.Entities;

public class Receita
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public DateOnly DataRecebimento { get; private set; }
    public string Categoria { get; private set; } = string.Empty;

    /// <summary>
    /// Primeiro dia do mês ao qual esta receita pertence para fins de resumo/relatórios.
    /// Igual ao mês de <see cref="DataRecebimento"/>, exceto quando o usuário optou por
    /// registrar a receita para o mês seguinte (ex.: recebimento em 26/09 lançado para outubro).
    /// </summary>
    public DateOnly MesReferencia { get; private set; }

    public DateTime DataCriacao { get; private set; }

    // EF Core
    private Receita() { }

    public static Receita Criar(
        Guid usuarioId,
        string descricao,
        decimal valor,
        DateOnly dataRecebimento,
        string categoria,
        bool registrarParaProximoMes = false)
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
            MesReferencia = CalcularMesReferencia(dataRecebimento, registrarParaProximoMes),
            DataCriacao = DateTime.UtcNow
        };
    }

    public void Atualizar(string descricao, decimal valor, DateOnly dataRecebimento, string categoria, bool registrarParaProximoMes)
    {
        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length < 3 || descricao.Length > 100)
            throw new ArgumentException("Descrição deve ter entre 3 e 100 caracteres.", nameof(descricao));

        if (valor <= 0)
            throw new ArgumentException("Valor deve ser maior que zero.", nameof(valor));

        Descricao = descricao.Trim();
        Valor = valor;
        DataRecebimento = dataRecebimento;
        Categoria = categoria;
        MesReferencia = CalcularMesReferencia(dataRecebimento, registrarParaProximoMes);
    }

    private static DateOnly CalcularMesReferencia(DateOnly dataRecebimento, bool registrarParaProximoMes)
    {
        var inicioMes = new DateOnly(dataRecebimento.Year, dataRecebimento.Month, 1);
        return registrarParaProximoMes ? inicioMes.AddMonths(1) : inicioMes;
    }
}
