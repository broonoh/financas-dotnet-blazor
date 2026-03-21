using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Application.Commands.Despesas;

public record CriarDespesaFixaCommand(
    Guid UsuarioId,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataPrimeiraParcela,
    CategoriaDespesa Categoria,
    FormaPagamentoDespesaFixa FormaPagamento) : IRequest<DespesaFixaDto>;
