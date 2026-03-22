using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Dividas;

public record AtualizarDividaCommand(
    Guid Id,
    Guid UsuarioId,
    string NomeDevedor,
    string Descricao) : IRequest<DividaDto>;
