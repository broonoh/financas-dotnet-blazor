using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.Commands.Despesas;

public record CriarDespesaFixaCommand(
    Guid UsuarioId,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela,
    string Categoria,
    FormaPagamentoDespesaFixa FormaPagamento) : IRequest<DespesaFixaDto>;
