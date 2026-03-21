namespace MinhasFinancas.Application.DTOs;

public record DashboardDto(
    decimal SaldoMesAtual,
    decimal TotalReceitasMes,
    decimal TotalDespesasMes,
    decimal ProjecaoProximos30Dias,
    List<DadosMensaisDto> Evolucao12Meses,
    List<CategoriaValorDto> DistribuicaoCategorias,
    List<DadosMensaisDto> Comparativo6Meses);

public record DadosMensaisDto(string Mes, decimal Receitas, decimal Despesas, decimal Saldo);
public record CategoriaValorDto(string Categoria, decimal Valor, decimal Percentual);
