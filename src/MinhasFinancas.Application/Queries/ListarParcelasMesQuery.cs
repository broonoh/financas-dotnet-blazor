using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarParcelasMesQuery(Guid UsuarioId, int Ano, int Mes) : IRequest<IEnumerable<ParcelaDto>>;
