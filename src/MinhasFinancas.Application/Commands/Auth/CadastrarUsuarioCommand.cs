using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Auth;

public record CadastrarUsuarioCommand(
    string Nome,
    string Email,
    string Senha,
    DateOnly DataNascimento,
    string? Telefone = null) : IRequest<UsuarioDto>;
