using MediatR;

namespace MinhasFinancas.Application.Commands.FormasPagamento;

public record ExcluirFormaPagamentoCommand(Guid Id, Guid UsuarioId) : IRequest;
