using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarCredoresQuery(Guid UsuarioId) : IRequest<IEnumerable<CredorDto>>;
