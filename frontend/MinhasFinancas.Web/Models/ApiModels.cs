namespace MinhasFinancas.Web.Models;

public record LoginRequest(string Email, string Senha);
public record LoginResponse(string AccessToken, string NomeUsuario, Guid UsuarioId);

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

public record ParcelaDto(
    Guid Id,
    Guid DespesaId,
    string DescricaoDespesa,
    int Numero,
    int TotalParcelas,
    decimal Valor,
    DateOnly DataVencimento,
    bool Paga,
    DateOnly? DataPagamento,
    bool Vencida,
    bool VenceEm7Dias);

public record ReceitaDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    string Categoria,
    bool Recorrente,
    DateTime DataCriacao);

public record DespesaFixaListDto(
    Guid Id,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela,
    string Categoria,
    string FormaPagamento,
    DateTime DataCriacao,
    List<ParcelaDto> Parcelas);

public record DespesaExtraListDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    string FormaPagamento,
    DateTime DataCriacao);

public record ParcelaDividaDto(
    Guid Id,
    Guid DividaId,
    int Numero,
    int TotalParcelas,
    decimal Valor,
    DateOnly DataVencimento,
    bool Paga,
    DateOnly? DataPagamento,
    bool Vencida);

public record DividaDto(
    Guid Id,
    string NomeDevedor,
    string Descricao,
    decimal ValorTotal,
    decimal SaldoRestante,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    bool Ativa,
    DateTime DataCriacao,
    List<ParcelaDividaDto> Parcelas);

public record PerfilDto(Guid Id, string Nome, string Email, DateOnly DataNascimento, string? Telefone, DateTime DataCadastro);

public record CategoriaDto(Guid Id, string Nome, DateTime DataCriacao);

public record ResumoMensalDto(
    int Ano,
    int Mes,
    List<ResumoItemDespesaFixaDto> DespesasFixas,
    List<ResumoItemDespesaExtraDto> DespesasExtras,
    List<ResumoDevedorDto> ContasAReceber,
    decimal TotalDespesasFixas,
    decimal TotalDespesasExtras,
    decimal TotalContasAReceber);

public record ResumoItemDespesaFixaDto(
    Guid ParcelaId,
    Guid DespesaId,
    string Descricao,
    string Categoria,
    int NumeroParcela,
    int TotalParcelas,
    decimal Valor,
    DateOnly DataVencimento,
    bool Paga);

public record ResumoItemDespesaExtraDto(
    Guid Id,
    string Descricao,
    string Categoria,
    decimal Valor,
    DateOnly DataDespesa);

public record ResumoDevedorDto(
    string NomeDevedor,
    List<ResumoParcelaDevedorDto> Parcelas,
    decimal Total);

public record ResumoParcelaDevedorDto(
    Guid ParcelaId,
    Guid DividaId,
    string Descricao,
    int NumeroParcela,
    int TotalParcelas,
    decimal Valor,
    DateOnly DataVencimento,
    bool Paga);
