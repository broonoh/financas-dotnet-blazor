using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarDevedoresQueryHandler : IRequestHandler<ListarDevedoresQuery, IEnumerable<DevedorDto>>
{
    private readonly IDevedorRepository _repo;

    public ListarDevedoresQueryHandler(IDevedorRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<DevedorDto>> Handle(ListarDevedoresQuery request, CancellationToken cancellationToken)
    {
        var devedores = await _repo.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken);
        return devedores.Select(d => new DevedorDto(d.Id, d.Nome, d.DataCriacao));
    }
}
