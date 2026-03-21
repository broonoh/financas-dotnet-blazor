using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.Commands.Despesas;

public record AtualizarDespesaExtraCommand(
    Guid Id,
    Guid UsuarioId,
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    string Categoria,
    FormaPagamentoDespesaExtra FormaPagamento) : IRequest<DespesaExtraDto>;
