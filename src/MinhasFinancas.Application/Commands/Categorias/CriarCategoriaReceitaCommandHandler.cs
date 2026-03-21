using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Categorias;

public class CriarCategoriaReceitaCommandHandler : IRequestHandler<CriarCategoriaReceitaCommand, CategoriaDto>
{
    private readonly ICategoriaReceitaRepository _repo;
    private readonly IUnitOfWork _uow;

    public CriarCategoriaReceitaCommandHandler(ICategoriaReceitaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<CategoriaDto> Handle(CriarCategoriaReceitaCommand request, CancellationToken cancellationToken)
    {
        var categoria = CategoriaReceita.Criar(request.UsuarioId, request.Nome);
        await _repo.AdicionarAsync(categoria, cancellationToken);
        await _uow.CommitAsync(cancellationToken);
        return new CategoriaDto(categoria.Id, categoria.Nome, categoria.DataCriacao);
    }
}
