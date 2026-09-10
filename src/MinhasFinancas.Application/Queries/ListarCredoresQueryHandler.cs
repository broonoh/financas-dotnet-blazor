using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarCredoresQueryHandler : IRequestHandler<ListarCredoresQuery, IEnumerable<CredorDto>>
{
    private readonly ICredorRepository _repo;

    public ListarCredoresQueryHandler(ICredorRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CredorDto>> Handle(ListarCredoresQuery request, CancellationToken cancellationToken)
    {
        var credores = await _repo.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken);
        return credores.Select(c => new CredorDto(c.Id, c.Nome, c.DataCriacao));
    }
}
