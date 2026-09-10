using MediatR;

namespace MinhasFinancas.Application.Commands.Despesas;

public record MarcarTodasDespesasExtrasPagasDoMesCommand(Guid UsuarioId, int Ano, int Mes) : IRequest<int>;
