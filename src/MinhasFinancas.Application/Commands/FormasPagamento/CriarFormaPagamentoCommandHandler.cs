using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.FormasPagamento;

public class CriarFormaPagamentoCommandHandler : IRequestHandler<CriarFormaPagamentoCommand, FormaPagamentoDto>
{
    private readonly IFormaPagamentoRepository _repo;
    private readonly IUnitOfWork _uow;

    public CriarFormaPagamentoCommandHandler(IFormaPagamentoRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<FormaPagamentoDto> Handle(CriarFormaPagamentoCommand request, CancellationToken cancellationToken)
    {
        var formaPagamento = FormaPagamento.Criar(request.UsuarioId, request.Nome);
        await _repo.AdicionarAsync(formaPagamento, cancellationToken);
        await _uow.CommitAsync(cancellationToken);
        return new FormaPagamentoDto(formaPagamento.Id, formaPagamento.Nome, formaPagamento.DataCriacao);
    }
}
