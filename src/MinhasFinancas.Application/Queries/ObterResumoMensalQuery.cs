using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ObterResumoMensalQuery(Guid UsuarioId, int Ano, int Mes) : IRequest<ResumoMensalDto>;
