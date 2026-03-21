using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Despesas;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using System.Security.Claims;

namespace MinhasFinancas.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DespesasController : ControllerBase
{
    private readonly IMediator _mediator;

    public DespesasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("fixas")]
    [ProducesResponseType(typeof(IEnumerable<DespesaFixaDto>), 200)]
    public async Task<IActionResult> ListarFixas(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarDespesasFixasQuery(usuarioId.Value), ct);
        return Ok(resultado);
    }

    [HttpGet("extras")]
    [ProducesResponseType(typeof(IEnumerable<DespesaExtraDto>), 200)]
    public async Task<IActionResult> ListarExtras(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        var resultado = await _mediator.Send(new ListarDespesasExtrasQuery(usuarioId.Value), ct);
        return Ok(resultado);
    }

    [HttpPost("fixas")]
    [ProducesResponseType(typeof(DespesaFixaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CriarFixa([FromBody] CriarDespesaFixaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new CriarDespesaFixaCommand(
                usuarioId.Value,
                request.Descricao,
                request.ValorTotal,
                request.QuantidadeParcelas,
                request.DataPrimeiraParcela,
                request.Categoria,
                request.FormaPagamento);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(CriarFixa), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("extras")]
    [ProducesResponseType(typeof(DespesaExtraDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CriarExtra([FromBody] CriarDespesaExtraRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            var command = new CriarDespesaExtraCommand(
                usuarioId.Value,
                request.Descricao,
                request.Valor,
                request.DataDespesa,
                request.Categoria,
                request.FormaPagamento);

            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(CriarExtra), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
    }

    [HttpPatch("parcelas/{parcelaId:guid}/pagar")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarParcela(Guid parcelaId, [FromBody] MarcarParcelaRequest request, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new MarcarParcelaPagaCommand(parcelaId, usuarioId.Value, request.Paga, request.DataPagamento), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("fixas/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExcluirFixa(Guid id, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new ExcluirDespesaCommand(id, usuarioId.Value, Fixa: true), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("extras/{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExcluirExtra(Guid id, CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        try
        {
            await _mediator.Send(new ExcluirDespesaCommand(id, usuarioId.Value, Fixa: false), ct);
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

// Request DTOs (separados do domínio)
public record CriarDespesaFixaRequest(
    string Descricao,
    decimal ValorTotal,
    int QuantidadeParcelas,
    DateOnly DataPrimeiraParcela,
    MinhasFinancas.Domain.Enums.CategoriaDespesa Categoria,
    MinhasFinancas.Domain.Enums.FormaPagamentoDespesaFixa FormaPagamento);

public record CriarDespesaExtraRequest(
    string Descricao,
    decimal Valor,
    DateOnly DataDespesa,
    MinhasFinancas.Domain.Enums.CategoriaDespesa Categoria,
    MinhasFinancas.Domain.Enums.FormaPagamentoDespesaExtra FormaPagamento);

public record MarcarParcelaRequest(bool Paga, DateOnly? DataPagamento = null);
