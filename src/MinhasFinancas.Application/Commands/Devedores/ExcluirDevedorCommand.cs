using MediatR;

namespace MinhasFinancas.Application.Commands.Devedores;

public record ExcluirDevedorCommand(Guid Id, Guid UsuarioId) : IRequest;
