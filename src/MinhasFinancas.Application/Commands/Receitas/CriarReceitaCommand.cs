using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.Commands.Receitas;

public record CriarReceitaCommand(
    Guid UsuarioId,
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    CategoriaReceita Categoria,
    bool Recorrente = false) : IRequest<ReceitaDto>;
