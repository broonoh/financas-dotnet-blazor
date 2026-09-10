using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarFormasPagamentoQuery(Guid UsuarioId) : IRequest<IEnumerable<FormaPagamentoDto>>;
