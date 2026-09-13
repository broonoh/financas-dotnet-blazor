using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Receitas;

public record CriarReceitaCommand(
    Guid UsuarioId,
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    string Categoria,
    bool RegistrarParaProximoMes = false) : IRequest<ReceitaDto>;
