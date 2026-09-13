namespace MinhasFinancas.Application.DTOs;

public record ReceitaDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    string Categoria,
    DateOnly MesReferencia,
    DateTime DataCriacao);
