using MediatR;

namespace MinhasFinancas.Application.Commands.Dividas;

public record MarcarTodasParcelasDividaPagasDoMesCommand(Guid UsuarioId, int Ano, int Mes) : IRequest<int>;
