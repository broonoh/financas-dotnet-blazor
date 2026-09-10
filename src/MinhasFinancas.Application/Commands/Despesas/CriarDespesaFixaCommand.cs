using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Despesas;

public record CriarDespesaFixaCommand(
    Guid UsuarioId,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela,
    string Categoria,
    string FormaPagamento,
    Guid? CredorId = null) : IRequest<DespesaFixaDto>;
