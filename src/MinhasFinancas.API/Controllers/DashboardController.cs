using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using System.Security.Claims;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), 200)]
    public async Task<IActionResult> ObterDashboard([FromQuery] int? ano, [FromQuery] int? mes, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var hoje = DateTime.UtcNow;
        var anoParam = ano ?? hoje.Year;
        var mesParam = mes ?? hoje.Month;

        var resultado = await _mediator.Send(new ObterDashboardQuery(usuarioId.Value, anoParam, mesParam), ct);
        return Ok(resultado);
    }

    [HttpGet("parcelas")]
    [ProducesResponseType(typeof(IEnumerable<ParcelaDto>), 200)]
    public async Task<IActionResult> ListarParcelas([FromQuery] int? ano, [FromQuery] int? mes, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var hoje = DateTime.UtcNow;
        var anoParam = ano ?? hoje.Year;
        var mesParam = mes ?? hoje.Month;

        var resultado = await _mediator.Send(new ListarParcelasMesQuery(usuarioId.Value, anoParam, mesParam), ct);
        return Ok(resultado);
    }

    private Guid? ObterUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
