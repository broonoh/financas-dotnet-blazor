using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class MarcarTodasParcelasFixasPagasDoMesCommandHandler : IRequestHandler<MarcarTodasParcelasFixasPagasDoMesCommand, int>
{
    private readonly IParcelaRepository _parcelaRepo;
    private readonly IUnitOfWork _uow;

    public MarcarTodasParcelasFixasPagasDoMesCommandHandler(IParcelaRepository parcelaRepo, IUnitOfWork uow)
    {
        _parcelaRepo = parcelaRepo;
        _uow = uow;
    }

    public async Task<int> Handle(MarcarTodasParcelasFixasPagasDoMesCommand request, CancellationToken cancellationToken)
    {
        var parcelas = (await _parcelaRepo.ListarPorUsuarioMesAsync(request.UsuarioId, request.Ano, request.Mes, cancellationToken))
            .Where(p => !p.Paga)
            .ToList();

        foreach (var parcela in parcelas)
        {
            parcela.MarcarPaga();
            _parcelaRepo.Atualizar(parcela);
        }

        if (parcelas.Count > 0)
            await _uow.CommitAsync(cancellationToken);

        return parcelas.Count;
    }
}
