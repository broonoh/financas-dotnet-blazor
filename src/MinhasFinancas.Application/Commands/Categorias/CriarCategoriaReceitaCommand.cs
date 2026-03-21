using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Categorias;

public record CriarCategoriaReceitaCommand(Guid UsuarioId, string Nome) : IRequest<CategoriaDto>;
