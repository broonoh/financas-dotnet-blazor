using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class CriarDespesaFixaCommandHandler : IRequestHandler<CriarDespesaFixaCommand, DespesaFixaDto>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly IUnitOfWork _uow;

    public CriarDespesaFixaCommandHandler(IDespesaRepository despesaRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _uow = uow;
    }

    public async Task<DespesaFixaDto> Handle(CriarDespesaFixaCommand request, CancellationToken cancellationToken)
    {
        var despesa = DespesaFixa.Criar(
            request.UsuarioId,
            request.Descricao,
            request.ValorTotal,
            request.QuantidadeParcelas,
            request.DataCompra,
            request.DataPrimeiraParcela,
            request.Categoria,
            request.FormaPagamento);

        await _despesaRepo.AdicionarFixaAsync(despesa, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return new DespesaFixaDto(
            despesa.Id,
            despesa.Descricao,
            despesa.ValorTotal,
            despesa.QuantidadeParcelas,
            despesa.DataCompra,
            despesa.DataPrimeiraParcela,
            despesa.Categoria,
            despesa.FormaPagamento,
            despesa.DataCriacao,
            []);
    }
}
