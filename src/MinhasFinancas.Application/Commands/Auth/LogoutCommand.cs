using MediatR;

namespace MinhasFinancas.Application.Commands.Auth;

public record LogoutCommand(Guid UsuarioId) : IRequest;
