using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.DTOs;

public record DespesaFixaDto(
    Guid Id,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela,
    string Categoria,
    FormaPagamentoDespesaFixa FormaPagamento,
    DateTime DataCriacao,
    List<ParcelaDto> Parcelas);

public record DespesaExtraDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    FormaPagamentoDespesaExtra FormaPagamento,
    DateTime DataCriacao);
