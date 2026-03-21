using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Categorias;

public record CriarCategoriaDespesaCommand(Guid UsuarioId, string Nome) : IRequest<CategoriaDto>;
