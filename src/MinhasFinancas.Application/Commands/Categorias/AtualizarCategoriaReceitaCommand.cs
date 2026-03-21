using MediatR;
using MinhasFinancas.Application.DTOs;

namespace MinhasFinancas.Application.Commands.Categorias;

public record AtualizarCategoriaReceitaCommand(Guid Id, Guid UsuarioId, string Nome) : IRequest<CategoriaDto>;
