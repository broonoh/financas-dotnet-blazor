using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarCategoriasReceitaQueryHandler : IRequestHandler<ListarCategoriasReceitaQuery, IEnumerable<CategoriaDto>>
{
    private readonly ICategoriaReceitaRepository _repo;

    public ListarCategoriasReceitaQueryHandler(ICategoriaReceitaRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CategoriaDto>> Handle(ListarCategoriasReceitaQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _repo.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken);
        return categorias.Select(c => new CategoriaDto(c.Id, c.Nome, c.DataCriacao));
    }
}
