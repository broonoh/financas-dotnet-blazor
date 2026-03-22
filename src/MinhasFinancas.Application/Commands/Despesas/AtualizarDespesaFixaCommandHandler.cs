using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class AtualizarDespesaFixaCommandHandler : IRequestHandler<AtualizarDespesaFixaCommand, DespesaFixaDto>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly IUnitOfWork _uow;

    public AtualizarDespesaFixaCommandHandler(IDespesaRepository despesaRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _uow = uow;
    }

    public async Task<DespesaFixaDto> Handle(AtualizarDespesaFixaCommand request, CancellationToken cancellationToken)
    {
        var despesa = await _despesaRepo.ObterDespesaFixaPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Despesa fixa não encontrada.");

        despesa.Atualizar(request.Descricao, request.Categoria, request.FormaPagamento);
        _despesaRepo.AtualizarFixa(despesa);
        await _uow.CommitAsync(cancellationToken);

        return new DespesaFixaDto(despesa.Id, despesa.Descricao, despesa.ValorTotal, despesa.QuantidadeParcelas,
            despesa.DataCompra, despesa.DataPrimeiraParcela, despesa.Categoria, despesa.FormaPagamento, despesa.DataCriacao);
    }
}
