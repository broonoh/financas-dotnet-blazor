using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class CriarDespesaExtraCommandHandler : IRequestHandler<CriarDespesaExtraCommand, DespesaExtraDto>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly IUnitOfWork _uow;

    public CriarDespesaExtraCommandHandler(IDespesaRepository despesaRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _uow = uow;
    }

    public async Task<DespesaExtraDto> Handle(CriarDespesaExtraCommand request, CancellationToken cancellationToken)
    {
        var despesa = DespesaExtra.Criar(
            request.UsuarioId,
            request.Descricao,
            request.Valor,
            request.DataDespesa,
            request.Categoria,
            request.FormaPagamento);

        await _despesaRepo.AdicionarExtraAsync(despesa, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return new DespesaExtraDto(
            despesa.Id,
            despesa.Descricao,
            despesa.ValorTotal,
            despesa.DataDespesa,
            despesa.Categoria,
            despesa.FormaPagamento,
            despesa.DataCriacao);
    }
}
