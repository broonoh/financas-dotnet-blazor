using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarCategoriasDespesaQueryHandler : IRequestHandler<ListarCategoriasDespesaQuery, IEnumerable<CategoriaDto>>
{
    private readonly ICategoriaDespesaRepository _repo;

    public ListarCategoriasDespesaQueryHandler(ICategoriaDespesaRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CategoriaDto>> Handle(ListarCategoriasDespesaQuery request, CancellationToken cancellationToken)
    {
        var categorias = await _repo.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken);
        return categorias.Select(c => new CategoriaDto(c.Id, c.Nome, c.DataCriacao));
    }
}
