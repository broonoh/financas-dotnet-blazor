using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Receitas;

public class ExcluirReceitaCommandHandler : IRequestHandler<ExcluirReceitaCommand>
{
    private readonly IReceitaRepository _receitaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirReceitaCommandHandler(IReceitaRepository receitaRepository, IUnitOfWork unitOfWork)
    {
        _receitaRepository = receitaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ExcluirReceitaCommand request, CancellationToken cancellationToken)
    {
        var receita = await _receitaRepository.ObterPorIdAsync(request.ReceitaId, request.UsuarioId)
            ?? throw new KeyNotFoundException("Receita não encontrada.");

        _receitaRepository.Remover(receita);
        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
