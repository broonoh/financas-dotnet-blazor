using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarDevedoresQuery(Guid UsuarioId) : IRequest<IEnumerable<DevedorDto>>;
