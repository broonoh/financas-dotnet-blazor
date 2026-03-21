using MediatR;

namespace MinhasFinancas.Application.Commands.Despesas;

public record ExcluirDespesaCommand(Guid DespesaId, Guid UsuarioId, bool Fixa) : IRequest;
