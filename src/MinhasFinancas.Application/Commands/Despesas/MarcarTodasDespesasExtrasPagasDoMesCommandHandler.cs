using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class MarcarTodasDespesasExtrasPagasDoMesCommandHandler : IRequestHandler<MarcarTodasDespesasExtrasPagasDoMesCommand, int>
{
    private readonly IDespesaRepository _despesaRepo;
    private readonly IUnitOfWork _uow;

    public MarcarTodasDespesasExtrasPagasDoMesCommandHandler(IDespesaRepository despesaRepo, IUnitOfWork uow)
    {
        _despesaRepo = despesaRepo;
        _uow = uow;
    }

    public async Task<int> Handle(MarcarTodasDespesasExtrasPagasDoMesCommand request, CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var despesas = (await _despesaRepo.ListarExtrasDoMesAsync(request.UsuarioId, request.Ano, request.Mes, cancellationToken))
            .Where(d => !d.Paga)
            .ToList();

        foreach (var despesa in despesas)
        {
            despesa.MarcarComoPaga(true, hoje);
            _despesaRepo.AtualizarExtra(despesa);
        }

        if (despesas.Count > 0)
            await _uow.CommitAsync(cancellationToken);

        return despesas.Count;
    }
}
