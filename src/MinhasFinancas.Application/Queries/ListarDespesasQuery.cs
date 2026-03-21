using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarDespesasFixasQuery(Guid UsuarioId) : IRequest<IEnumerable<DespesaFixaDto>>;
public record ListarDespesasExtrasQuery(Guid UsuarioId) : IRequest<IEnumerable<DespesaExtraDto>>;
