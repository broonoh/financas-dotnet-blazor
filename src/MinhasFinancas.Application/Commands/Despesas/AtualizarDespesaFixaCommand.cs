using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.Commands.Despesas;

public record AtualizarDespesaFixaCommand(
    Guid Id,
    Guid UsuarioId,
    string Descricao,
    string Categoria,
    FormaPagamentoDespesaFixa FormaPagamento) : IRequest<DespesaFixaDto>;
