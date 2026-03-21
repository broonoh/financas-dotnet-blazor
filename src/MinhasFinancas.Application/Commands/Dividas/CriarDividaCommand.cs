using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Dividas;

public record CriarDividaCommand(
    Guid UsuarioId,
    string NomeDevedor,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra) : IRequest<DividaDto>;
