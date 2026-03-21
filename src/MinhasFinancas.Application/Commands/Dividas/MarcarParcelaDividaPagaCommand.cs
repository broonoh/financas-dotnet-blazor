using MediatR;

namespace MinhasFinancas.Application.Commands.Dividas;

public record MarcarParcelaDividaPagaCommand(
    Guid ParcelaId,
    Guid UsuarioId,
    bool Paga,
    DateOnly? DataPagamento = null) : IRequest;
