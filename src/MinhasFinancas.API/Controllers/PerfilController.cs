using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Auth;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using System.Security.Claims;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PerfilController : ControllerBase
{
    private readonly IMediator _mediator;
    public PerfilController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(PerfilDto), 200)]
    public async Task<IActionResult> Obter(CancellationToken ct)
    {
        var id = ObterUsuarioId();
        if (id == null) return Unauthorized();
        try { return Ok(await _mediator.Send(new ObterPerfilQuery(id.Value), ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPut]
    [ProducesResponseType(typeof(PerfilDto), 200)]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarPerfilRequest req, CancellationToken ct)
    {
        var id = ObterUsuarioId();
        if (id == null) return Unauthorized();
        try { return Ok(await _mediator.Send(new AtualizarPerfilCommand(id.Value, req.Nome, req.Telefone), ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    private Guid? ObterUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record AtualizarPerfilRequest(string Nome, string? Telefone);
