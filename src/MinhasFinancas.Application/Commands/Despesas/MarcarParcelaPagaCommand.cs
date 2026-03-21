using MediatR;

namespace MinhasFinancas.Application.Commands.Despesas;

public record MarcarParcelaPagaCommand(Guid ParcelaId, Guid UsuarioId, bool Paga, DateOnly? DataPagamento = null) : IRequest;
