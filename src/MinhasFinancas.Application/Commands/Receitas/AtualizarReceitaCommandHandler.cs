using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Receitas;

public class AtualizarReceitaCommandHandler : IRequestHandler<AtualizarReceitaCommand, ReceitaDto>
{
    private readonly IReceitaRepository _receitaRepo;
    private readonly IUnitOfWork _uow;

    public AtualizarReceitaCommandHandler(IReceitaRepository receitaRepo, IUnitOfWork uow)
    {
        _receitaRepo = receitaRepo;
        _uow = uow;
    }

    public async Task<ReceitaDto> Handle(AtualizarReceitaCommand request, CancellationToken cancellationToken)
    {
        var receita = await _receitaRepo.ObterPorIdAsync(request.Id, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Receita não encontrada.");

        receita.Atualizar(request.Descricao, request.Valor, request.DataRecebimento, request.Categoria);
        _receitaRepo.Atualizar(receita);
        await _uow.CommitAsync(cancellationToken);

        return new ReceitaDto(receita.Id, receita.Descricao, receita.Valor, receita.DataRecebimento, receita.Categoria, receita.Recorrente, receita.DataCriacao);
    }
}
