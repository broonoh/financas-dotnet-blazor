using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.FormasPagamento;

public class AtualizarFormaPagamentoCommandHandler : IRequestHandler<AtualizarFormaPagamentoCommand, FormaPagamentoDto>
{
    private readonly IFormaPagamentoRepository _repo;
    private readonly IUnitOfWork _uow;

    public AtualizarFormaPagamentoCommandHandler(IFormaPagamentoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<FormaPagamentoDto> Handle(AtualizarFormaPagamentoCommand request, CancellationToken cancellationToken)
    {
        var formaPagamento = await _repo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Forma de pagamento não encontrada.");

        formaPagamento.Atualizar(request.Nome);
        _repo.Atualizar(formaPagamento);
        await _uow.CommitAsync(cancellationToken);
        return new FormaPagamentoDto(formaPagamento.Id, formaPagamento.Nome, formaPagamento.DataCriacao);
    }
}
