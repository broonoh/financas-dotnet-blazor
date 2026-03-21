using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Despesas;

public class ExcluirDespesaCommandHandler : IRequestHandler<ExcluirDespesaCommand>
{
    private readonly IDespesaRepository _despesaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirDespesaCommandHandler(IDespesaRepository despesaRepository, IUnitOfWork unitOfWork)
    {
        _despesaRepository = despesaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ExcluirDespesaCommand request, CancellationToken cancellationToken)
    {
        if (request.Fixa)
        {
            var despesa = await _despesaRepository.ObterDespesaFixaPorIdAsync(request.DespesaId, request.UsuarioId)
                ?? throw new KeyNotFoundException("Despesa fixa não encontrada.");
            _despesaRepository.Remover(despesa);
        }
        else
        {
            var despesa = await _despesaRepository.ObterDespesaExtraPorIdAsync(request.DespesaId, request.UsuarioId)
                ?? throw new KeyNotFoundException("Despesa extra não encontrada.");
            _despesaRepository.Remover(despesa);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
