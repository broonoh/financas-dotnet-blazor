using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Receitas;

public record AtualizarReceitaCommand(
    Guid Id,
    Guid UsuarioId,
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    string Categoria) : IRequest<ReceitaDto>;
