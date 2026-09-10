using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Dividas;

public class CriarDividaCommandHandler : IRequestHandler<CriarDividaCommand, DividaDto>
{
    private readonly IDividaRepository _dividaRepository;
    private readonly IDevedorRepository _devedorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarDividaCommandHandler(IDividaRepository dividaRepository, IDevedorRepository devedorRepository, IUnitOfWork unitOfWork)
    {
        _dividaRepository = dividaRepository;
        _devedorRepository = devedorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DividaDto> Handle(CriarDividaCommand request, CancellationToken cancellationToken)
    {
        var devedor = await _devedorRepository.ObterPorIdAsync(request.DevedorId, request.UsuarioId, cancellationToken)
            ?? throw new KeyNotFoundException("Devedor não encontrado.");

        var divida = Divida.Criar(
            request.UsuarioId,
            request.DevedorId,
            request.Descricao,
            request.ValorTotal,
            request.QuantidadeParcelas,
            request.DataCompra,
            request.DataPrimeiraParcela);

        await _dividaRepository.AdicionarAsync(divida);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToDto(divida, devedor.Nome);
    }

    private static DividaDto MapToDto(Divida divida, string nomeDevedor)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var parcelas = divida.Parcelas.OrderBy(p => p.Numero).Select(p => new ParcelaDividaDto(
            p.Id, p.DividaId, p.Numero, divida.QuantidadeParcelas,
            p.Valor, p.DataVencimento, p.Paga, p.DataPagamento,
            !p.Paga && p.DataVencimento < hoje)).ToList();

        var saldoRestante = divida.Parcelas.Where(p => !p.Paga).Sum(p => p.Valor);

        return new DividaDto(
            divida.Id, divida.DevedorId, nomeDevedor, divida.Descricao, divida.ValorTotal,
            saldoRestante, divida.QuantidadeParcelas, divida.DataCompra,
            divida.DataPrimeiraParcela, divida.Ativa, divida.DataCriacao, parcelas);
    }
}
