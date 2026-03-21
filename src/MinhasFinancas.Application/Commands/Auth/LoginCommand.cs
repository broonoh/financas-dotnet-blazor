using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Auth;

public record LoginCommand(string Email, string Senha) : IRequest<LoginResponseDto>;
