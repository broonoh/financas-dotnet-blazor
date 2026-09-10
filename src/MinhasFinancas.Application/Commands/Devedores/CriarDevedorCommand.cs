using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Devedores;

public record CriarDevedorCommand(Guid UsuarioId, string Nome) : IRequest<DevedorDto>;
