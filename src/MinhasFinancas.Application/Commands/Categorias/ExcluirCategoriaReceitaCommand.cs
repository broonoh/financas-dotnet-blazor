using MediatR;

namespace MinhasFinancas.Application.Commands.Categorias;

public record ExcluirCategoriaReceitaCommand(Guid Id, Guid UsuarioId) : IRequest;
