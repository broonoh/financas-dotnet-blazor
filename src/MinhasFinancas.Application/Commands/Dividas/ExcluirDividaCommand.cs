using MediatR;

namespace MinhasFinancas.Application.Commands.Dividas;

public record ExcluirDividaCommand(Guid DividaId, Guid UsuarioId) : IRequest;
