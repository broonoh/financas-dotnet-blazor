using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.Commands.Despesas;

public record CriarDespesaExtraCommand(
    Guid UsuarioId,
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    CategoriaDespesa Categoria,
    FormaPagamentoDespesaExtra FormaPagamento) : IRequest<DespesaExtraDto>;
