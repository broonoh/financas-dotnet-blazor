namespace MinhasFinancas.Application.DTOs;

public record ParcelaDto(
    Guid Id,
    Guid DespesaId,
    string DescricaoDespesa,
    int Numero,
    int TotalParcelas,
    decimal Valor,
    DateOnly DataVencimento,
    bool Paga,
    DateOnly? DataPagamento,
    bool Vencida,
    bool VenceEm7Dias);
