using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Credores;

public record CriarCredorCommand(Guid UsuarioId, string Nome) : IRequest<CredorDto>;
