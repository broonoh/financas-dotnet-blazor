using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Auth;

public record RefreshTokenCommand(Guid UsuarioId, string RefreshToken) : IRequest<LoginResponseDto>;
