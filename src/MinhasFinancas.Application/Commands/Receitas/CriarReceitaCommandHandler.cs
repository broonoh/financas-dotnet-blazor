using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Receitas;

public class CriarReceitaCommandHandler : IRequestHandler<CriarReceitaCommand, ReceitaDto>
{
    private readonly IReceitaRepository _receitaRepo;
    private readonly IUnitOfWork _uow;

    public CriarReceitaCommandHandler(IReceitaRepository receitaRepo, IUnitOfWork uow)
    {
        _receitaRepo = receitaRepo;
        _uow = uow;
    }

    public async Task<ReceitaDto> Handle(CriarReceitaCommand request, CancellationToken cancellationToken)
    {
        var receita = Receita.Criar(
            request.UsuarioId,
            request.Descricao,
            request.Valor,
            request.DataRecebimento,
            request.Categoria,
            request.Recorrente);

        if (request.Recorrente)
        {
            // Gerar 12 meses de receitas recorrentes
            var receitas = new List<Receita> { receita };
            for (int i = 1; i < 12; i++)
            {
                var novaData = request.DataRecebimento.AddMonths(i);
                receitas.Add(Receita.Criar(
                    request.UsuarioId,
                    request.Descricao,
                    request.Valor,
                    novaData,
                    request.Categoria,
                    true));
            }
            await _receitaRepo.AdicionarVariasAsync(receitas, cancellationToken);
        }
        else
        {
            await _receitaRepo.AdicionarAsync(receita, cancellationToken);
        }

        await _uow.CommitAsync(cancellationToken);

        return new ReceitaDto(receita.Id, receita.Descricao, receita.Valor, receita.DataRecebimento, receita.Categoria, receita.Recorrente, receita.DataCriacao);
    }
}
