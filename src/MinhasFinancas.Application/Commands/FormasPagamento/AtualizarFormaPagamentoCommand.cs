using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.FormasPagamento;

public record AtualizarFormaPagamentoCommand(Guid Id, Guid UsuarioId, string Nome) : IRequest<FormaPagamentoDto>;
