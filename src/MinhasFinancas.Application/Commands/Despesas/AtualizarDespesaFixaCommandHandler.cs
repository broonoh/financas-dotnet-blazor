using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class AtualizarDespesaFixaCommandHandler : IRequestHandler<AtualizarDespesaFixaCommand, DespesaFixaDto>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly ICredorRepository _credorRepo;
    private readonly IUnitOfWork _uow;

    public AtualizarDespesaFixaCommandHandler(IDespesaRepository despesaRepo, ICredorRepository credorRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _credorRepo = credorRepo;
        _uow = uow;
    }

    public async Task<DespesaFixaDto> Handle(AtualizarDespesaFixaCommand request, CancellationToken cancellationToken)
    {
        var despesa = await _despesaRepo.ObterDespesaFixaPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Despesa fixa não encontrada.");

        string? nomeCredor = null;
        if (request.CredorId is Guid credorId)
        {
            var credor = await _credorRepo.ObterPorIdAsync(credorId, request.UsuarioId, cancellationToken)
                ?? throw new KeyNotFoundException("Credor não encontrado.");
            nomeCredor = credor.Nome;
        }

        despesa.Atualizar(request.Descricao, request.ValorTotal, request.QuantidadeParcelas, request.DataCompra, request.DataPrimeiraParcela, request.Categoria, request.FormaPagamento, request.CredorId);
        await _despesaRepo.AtualizarFixaAsync(despesa, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return new DespesaFixaDto(despesa.Id, despesa.Descricao, despesa.ValorTotal, despesa.QuantidadeParcelas,
            despesa.DataCompra, despesa.DataPrimeiraParcela, despesa.Categoria, despesa.FormaPagamento, despesa.DataCriacao, [],
            despesa.CredorId, nomeCredor);
    }
}
