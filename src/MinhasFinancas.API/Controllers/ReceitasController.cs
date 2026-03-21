using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Receitas;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Domain.Enums;
using System.Security.Claims;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReceitasController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReceitasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ReceitaDto>), 200)]
    public async Task<IActionResult> Listar([FromQuery] int? ano, [FromQuery] int? mes, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarReceitasQuery(usuarioId.Value, ano, mes), ct);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReceitaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarReceitaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new CriarReceitaCommand(
                usuarioId.Value,
                request.Descricao,
                request.Valor,
                request.DataRecebimento,
                request.Categoria,
                request.Recorrente);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(Criar), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new ExcluirReceitaCommand(id, usuarioId.Value), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid? ObterUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record CriarReceitaRequest(
    string Descricao,
    decimal Valor,
    DateOnly DataRecebimento,
    CategoriaReceita Categoria,
    bool Recorrente = false);
