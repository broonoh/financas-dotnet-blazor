using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Categorias;

public class AtualizarCategoriaDespesaCommandHandler : IRequestHandler<AtualizarCategoriaDespesaCommand, CategoriaDto>
{
    private readonly ICategoriaDespesaRepository _repo;
    private readonly IUnitOfWork _uow;

    public AtualizarCategoriaDespesaCommandHandler(ICategoriaDespesaRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<CategoriaDto> Handle(AtualizarCategoriaDespesaCommand request, CancellationToken cancellationToken)
    {
        var categoria = await _repo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Categoria não encontrada.");

        categoria.Atualizar(request.Nome);
        _repo.Atualizar(categoria);
        await _uow.CommitAsync(cancellationToken);
        return new CategoriaDto(categoria.Id, categoria.Nome, categoria.DataCriacao);
    }
}
