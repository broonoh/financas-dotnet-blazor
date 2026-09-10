using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Devedores;

public record AtualizarDevedorCommand(Guid Id, Guid UsuarioId, string Nome) : IRequest<DevedorDto>;
