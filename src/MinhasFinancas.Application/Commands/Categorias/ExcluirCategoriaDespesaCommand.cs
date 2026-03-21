using MediatR;

namespace MinhasFinancas.Application.Commands.Categorias;

public record ExcluirCategoriaDespesaCommand(Guid Id, Guid UsuarioId) : IRequest;
