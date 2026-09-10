using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.FormasPagamento;

public class ExcluirFormaPagamentoCommandHandler : IRequestHandler<ExcluirFormaPagamentoCommand>
{
    private readonly IFormaPagamentoRepository _repo;
    private readonly IUnitOfWork _uow;

    public ExcluirFormaPagamentoCommandHandler(IFormaPagamentoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task Handle(ExcluirFormaPagamentoCommand request, CancellationToken cancellationToken)
    {
        var formaPagamento = await _repo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Forma de pagamento não encontrada.");

        _repo.Remover(formaPagamento);
        await _uow.CommitAsync(cancellationToken);
    }
}
