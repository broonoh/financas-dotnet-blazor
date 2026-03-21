using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarDividasQuery(Guid UsuarioId) : IRequest<IEnumerable<DividaDto>>;
