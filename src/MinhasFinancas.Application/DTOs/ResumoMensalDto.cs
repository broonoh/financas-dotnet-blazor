namespace MinhasFinancas.Application.DTOs;

public record ResumoMensalDto(
    int Ano,
    int Mes,
    List<ResumoItemDespesaFixaDto> DespesasFixas,
    List<ResumoItemDespesaExtraDto> DespesasExtras,
    List<ResumoDevedorDto> ContasAReceber,
    List<ResumoItemReceitaDto> Receitas,
    decimal TotalDespesasFixas,
    decimal TotalDespesasExtras,
    decimal TotalContasAReceber,
    decimal TotalReceitas,
    decimal Saldo);

public record ResumoItemReceitaDto(
    Guid Id,
    string Descricao,
    string Categoria,
    decimal Valor,
    DateOnly DataRecebimento,
    bool Recorrente);

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
    DateOnly DataDespesa,
    DateOnly? PagaEm,
    bool Paga);

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
