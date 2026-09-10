using MediatR;
using MinhasFinancas.Application.DTOs;
namespace MinhasFinancas.Application.Commands.Auth;
public record AtualizarPerfilCommand(Guid UsuarioId, string Nome, string? Telefone, string? Email = null, DateOnly? DataNascimento = null) : IRequest<PerfilDto>;
