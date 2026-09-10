using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class CriarDespesaExtraCommandHandler : IRequestHandler<CriarDespesaExtraCommand, DespesaExtraDto>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly ICredorRepository _credorRepo;
    private readonly IUnitOfWork _uow;

    public CriarDespesaExtraCommandHandler(IDespesaRepository despesaRepo, ICredorRepository credorRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _credorRepo = credorRepo;
        _uow = uow;
    }

    public async Task<DespesaExtraDto> Handle(CriarDespesaExtraCommand request, CancellationToken cancellationToken)
    {
        string? nomeCredor = null;
        if (request.CredorId is Guid credorId)
        {
            var credor = await _credorRepo.ObterPorIdAsync(credorId, request.UsuarioId, cancellationToken)
                ?? throw new KeyNotFoundException("Credor não encontrado.");
            nomeCredor = credor.Nome;
        }

        var despesa = DespesaExtra.Criar(
            request.UsuarioId,
            request.Descricao,
            request.Valor,
            request.DataDespesa,
            request.Categoria,
            request.FormaPagamento,
            request.PagaEm,
            request.CredorId);

        await _despesaRepo.AdicionarExtraAsync(despesa, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return new DespesaExtraDto(
            despesa.Id,
            despesa.Descricao,
            despesa.ValorTotal,
            despesa.DataDespesa,
            despesa.Categoria,
            despesa.FormaPagamento,
            despesa.PagaEm,
            despesa.Paga,
            despesa.DataCriacao,
            despesa.CredorId,
            nomeCredor);
    }
}
