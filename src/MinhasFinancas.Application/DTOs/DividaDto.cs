namespace MinhasFinancas.Application.DTOs;

public record ParcelaDividaDto(
    Guid Id,
    Guid DividaId,
    int Numero,
    int TotalParcelas,
    decimal Valor,
    DateOnly DataVencimento,
    bool Paga,
    DateOnly? DataPagamento,
    bool Vencida);

public record DividaDto(
    Guid Id,
    string NomeDevedor,
    string Descricao,
    decimal ValorTotal,
    decimal SaldoRestante,
    int QuantidadeParcelas,
    DateOnly DataCompra,
    bool Ativa,
    DateTime DataCriacao,
    IEnumerable<ParcelaDividaDto> Parcelas);
