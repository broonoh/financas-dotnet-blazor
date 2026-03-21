using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ListarReceitasQuery(Guid UsuarioId, int? Ano = null, int? Mes = null) : IRequest<IEnumerable<ReceitaDto>>;
