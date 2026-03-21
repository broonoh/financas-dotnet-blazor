using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class AtualizarDespesaExtraCommandHandler : IRequestHandler<AtualizarDespesaExtraCommand, DespesaExtraDto>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly IUnitOfWork _uow;

    public AtualizarDespesaExtraCommandHandler(IDespesaRepository despesaRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _uow = uow;
    }

    public async Task<DespesaExtraDto> Handle(AtualizarDespesaExtraCommand request, CancellationToken cancellationToken)
    {
        var despesa = await _despesaRepo.ObterDespesaExtraPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Despesa extra não encontrada.");

        despesa.Atualizar(request.Descricao, request.Valor, request.DataDespesa, request.Categoria, request.FormaPagamento);
        _despesaRepo.AtualizarExtra(despesa);
        await _uow.CommitAsync(cancellationToken);

        return new DespesaExtraDto(despesa.Id, despesa.Descricao, despesa.ValorTotal, despesa.DataDespesa,
            despesa.Categoria, despesa.FormaPagamento, despesa.DataCriacao);
    }
}
