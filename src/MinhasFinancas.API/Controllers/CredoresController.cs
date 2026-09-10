using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Credores;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using System.Security.Claims;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CredoresController : ControllerBase
{
    private readonly IMediator _mediator;

    public CredoresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CredorDto>), 200)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarCredoresQuery(usuarioId.Value), ct);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CredorDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarCredorRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var resultado = await _mediator.Send(new CriarCredorCommand(usuarioId.Value, request.Nome), ct);
            return CreatedAtAction(nameof(Criar), new { id = resultado.Id }, resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CredorDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CriarCredorRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var resultado = await _mediator.Send(new AtualizarCredorCommand(id, usuarioId.Value, request.Nome), ct);
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
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new ExcluirCredorCommand(id, usuarioId.Value), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid? ObterUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record CriarCredorRequest(string Nome);
