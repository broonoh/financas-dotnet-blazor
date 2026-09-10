using MediatR;

namespace MinhasFinancas.Application.Commands.Despesas;

public record MarcarTodasParcelasFixasPagasDoMesCommand(Guid UsuarioId, int Ano, int Mes) : IRequest<int>;
