using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Categorias;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using System.Security.Claims;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/categorias/despesa")]
public class CategoriasDespesaController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriasDespesaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaDto>), 200)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarCategoriasDespesaQuery(usuarioId.Value), ct);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoriaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarCategoriaDespesaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var resultado = await _mediator.Send(new CriarCategoriaDespesaCommand(usuarioId.Value, request.Nome), ct);
            return CreatedAtAction(nameof(Criar), new { id = resultado.Id }, resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoriaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarCategoriaDespesaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var resultado = await _mediator.Send(new AtualizarCategoriaDespesaCommand(id, usuarioId.Value, request.Nome), ct);
            return Ok(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            await _mediator.Send(new ExcluirCategoriaDespesaCommand(id, usuarioId.Value), ct);
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

public record CriarCategoriaDespesaRequest(string Nome);
