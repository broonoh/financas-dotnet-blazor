using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarFormasPagamentoQueryHandler : IRequestHandler<ListarFormasPagamentoQuery, IEnumerable<FormaPagamentoDto>>
{
    private readonly IFormaPagamentoRepository _repo;

    public ListarFormasPagamentoQueryHandler(IFormaPagamentoRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<FormaPagamentoDto>> Handle(ListarFormasPagamentoQuery request, CancellationToken cancellationToken)
    {
        var formasPagamento = await _repo.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken);
        return formasPagamento.Select(f => new FormaPagamentoDto(f.Id, f.Nome, f.DataCriacao));
    }
}
