using MediatR;
using MinhasFinancas.Application.DTOs;
namespace MinhasFinancas.Application.Queries;
public record ObterPerfilQuery(Guid UsuarioId) : IRequest<PerfilDto>;
