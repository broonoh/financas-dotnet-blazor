using MediatR;

namespace MinhasFinancas.Application.Commands.Receitas;

public record ExcluirReceitaCommand(Guid ReceitaId, Guid UsuarioId) : IRequest;
