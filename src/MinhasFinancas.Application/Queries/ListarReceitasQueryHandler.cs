using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Queries;

public class ListarReceitasQueryHandler : IRequestHandler<ListarReceitasQuery, IEnumerable<ReceitaDto>>
{
    private readonly IReceitaRepository _receitaRepo;

    public ListarReceitasQueryHandler(IReceitaRepository receitaRepo)
    {
        _receitaRepo = receitaRepo;
    }

    public async Task<IEnumerable<ReceitaDto>> Handle(ListarReceitasQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<MinhasFinancas.Domain.Entities.Receita> receitas;

        if (request.Ano.HasValue && request.Mes.HasValue)
            receitas = await _receitaRepo.ListarPorUsuarioMesAsync(request.UsuarioId, request.Ano.Value, request.Mes.Value, cancellationToken);
        else
            receitas = await _receitaRepo.ListarPorUsuarioAsync(request.UsuarioId, cancellationToken);

        return receitas.Select(r => new ReceitaDto(
            r.Id, r.Descricao, r.Valor, r.DataRecebimento, r.Categoria, r.Recorrente, r.DataCriacao));
    }
}
