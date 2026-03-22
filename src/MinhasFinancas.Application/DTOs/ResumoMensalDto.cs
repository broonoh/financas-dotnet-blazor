namespace MinhasFinancas.Application.DTOs;

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
