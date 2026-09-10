using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.FormasPagamento;

public record CriarFormaPagamentoCommand(Guid UsuarioId, string Nome) : IRequest<FormaPagamentoDto>;
