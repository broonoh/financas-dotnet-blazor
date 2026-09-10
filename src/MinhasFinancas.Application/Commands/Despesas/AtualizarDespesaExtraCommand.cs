using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Despesas;

public record AtualizarDespesaExtraCommand(
    Guid Id,
    Guid UsuarioId,
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    string FormaPagamento,
    DateOnly? PagaEm = null,
    Guid? CredorId = null) : IRequest<DespesaExtraDto>;
