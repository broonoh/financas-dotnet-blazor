using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Categorias;

public record AtualizarCategoriaDespesaCommand(Guid Id, Guid UsuarioId, string Nome) : IRequest<CategoriaDto>;
