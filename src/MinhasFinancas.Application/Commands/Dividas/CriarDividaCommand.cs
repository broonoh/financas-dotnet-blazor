using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Dividas;

public record CriarDividaCommand(
    Guid UsuarioId,
    Guid DevedorId,
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    DateOnly DataPrimeiraParcela) : IRequest<DividaDto>;
