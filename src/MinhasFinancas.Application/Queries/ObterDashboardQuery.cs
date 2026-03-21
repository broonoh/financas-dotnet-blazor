using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Queries;

public record ObterDashboardQuery(Guid UsuarioId, int Ano, int Mes) : IRequest<DashboardDto>;
