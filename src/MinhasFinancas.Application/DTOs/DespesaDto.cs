using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.DTOs;

public record DespesaFixaDto(
    Guid Id,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataPrimeiraParcela,
    CategoriaDespesa Categoria,
    FormaPagamentoDespesaFixa FormaPagamento,
    DateTime DataCriacao);

public record DespesaExtraDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    CategoriaDespesa Categoria,
    FormaPagamentoDespesaExtra FormaPagamento,
    DateTime DataCriacao);
