using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarCategoriasDespesaQuery(Guid UsuarioId) : IRequest<IEnumerable<CategoriaDto>>;
