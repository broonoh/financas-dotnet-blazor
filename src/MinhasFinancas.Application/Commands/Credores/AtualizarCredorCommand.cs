using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Credores;

public record AtualizarCredorCommand(Guid Id, Guid UsuarioId, string Nome) : IRequest<CredorDto>;
