using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class CriarDespesaFixaCommandHandler : IRequestHandler<CriarDespesaFixaCommand, DespesaFixaDto>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly ICredorRepository _credorRepo;
    private readonly IUnitOfWork _uow;

    public CriarDespesaFixaCommandHandler(IDespesaRepository despesaRepo, ICredorRepository credorRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _credorRepo = credorRepo;
        _uow = uow;
    }

    public async Task<DespesaFixaDto> Handle(CriarDespesaFixaCommand request, CancellationToken cancellationToken)
    {
        string? nomeCredor = null;
        if (request.CredorId is Guid credorId)
        {
            var credor = await _credorRepo.ObterPorIdAsync(credorId, request.UsuarioId, cancellationToken)
                ?? throw new KeyNotFoundException("Credor não encontrado.");
            nomeCredor = credor.Nome;
        }

        var despesa = DespesaFixa.Criar(
            request.UsuarioId,
            request.Descricao,
            request.ValorParcela,
            request.QuantidadeParcelas,
            request.DataCompra,
            request.DataPrimeiraParcela,
            request.Categoria,
            request.FormaPagamento,
            request.CredorId);

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
            [],
            despesa.CredorId,
            nomeCredor);
    }
}
