using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarCategoriasReceitaQuery(Guid UsuarioId) : IRequest<IEnumerable<CategoriaDto>>;
