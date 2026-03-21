using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Categorias;

public class CriarCategoriaDespesaCommandHandler : IRequestHandler<CriarCategoriaDespesaCommand, CategoriaDto>
{
    private readonly ICategoriaDespesaRepository _repo;
    private readonly IUnitOfWork _uow;

    public CriarCategoriaDespesaCommandHandler(ICategoriaDespesaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<CategoriaDto> Handle(CriarCategoriaDespesaCommand request, CancellationToken cancellationToken)
    {
        var categoria = CategoriaDespesa.Criar(request.UsuarioId, request.Nome);
        await _repo.AdicionarAsync(categoria, cancellationToken);
        await _uow.CommitAsync(cancellationToken);
        return new CategoriaDto(categoria.Id, categoria.Nome, categoria.DataCriacao);
    }
}
