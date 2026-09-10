namespace MinhasFinancas.Application.DTOs;

public record DespesaFixaDto(
    Guid Id,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela,
    string Categoria,
    string FormaPagamento,
    DateTime DataCriacao,
    List<ParcelaDto> Parcelas,
    Guid? CredorId = null,
    string? NomeCredor = null);

public record DespesaExtraDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    string FormaPagamento,
    DateOnly? PagaEm,
    bool Paga,
    DateTime DataCriacao,
    Guid? CredorId = null,
    string? NomeCredor = null);
