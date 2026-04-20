using MediatR;

namespace MinhasFinancas.Application.Commands.Despesas;

public record MarcarDespesaExtraPagaCommand(Guid Id, Guid UsuarioId, bool Paga) : IRequest;