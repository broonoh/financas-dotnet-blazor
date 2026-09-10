using MediatR;

namespace MinhasFinancas.Application.Commands.Credores;

public record ExcluirCredorCommand(Guid Id, Guid UsuarioId) : IRequest;
