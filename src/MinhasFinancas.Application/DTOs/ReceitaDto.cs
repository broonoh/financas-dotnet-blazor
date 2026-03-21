using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.DTOs;

public record ReceitaDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    CategoriaReceita Categoria,
    bool Recorrente,
    DateTime DataCriacao);
