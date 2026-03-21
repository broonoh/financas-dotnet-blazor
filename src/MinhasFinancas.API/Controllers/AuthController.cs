using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinhasFinancas.Application.Commands.Auth;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Infrastructure.Services;
using System.Security.Claims;

namespace MinhasFinancas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly LoginAttemptService _loginAttempts;

    public AuthController(IMediator mediator, LoginAttemptService loginAttempts)
    {
        _mediator = mediator;
        _loginAttempts = loginAttempts;
    }

    [HttpPost("cadastrar")]
    [ProducesResponseType(typeof(UsuarioDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastrarUsuarioCommand command, CancellationToken ct)
    {
        try
        {
            var resultado = await _mediator.Send(command, ct);
            return CreatedAtAction(nameof(Cadastrar), new { id = resultado.Id }, resultado);
        }
        catch (FluentValidation.ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var email = command.Email;

        // Rate limiting check
        if (_loginAttempts.EstaBloqueado(email))
        {
            var tempoRestante = _loginAttempts.TempoRestanteBloqueio(email);
            return StatusCode(429, new { message = $"Conta bloqueada. Tente novamente em {tempoRestante?.Minutes} minutos." });
        }

        try
        {
            var resultado = await _mediator.Send(command, ct);
            _loginAttempts.LimparTentativas(email);

            // Refresh token em HttpOnly Cookie
            Response.Cookies.Append("refreshToken", resultado.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(resultado);
        }
        catch (UnauthorizedAccessException)
        {
            _loginAttempts.RegistrarTentativaFalha(email);
            return Unauthorized(new { message = "Credenciais inválidas." });
        }
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        try
        {
            var resultado = await _mediator.Send(command, ct);
            Response.Cookies.Append("refreshToken", resultado.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            return Ok(resultado);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var usuarioId = ObterUsuarioId();
        if (usuarioId == null) return Unauthorized();

        await _mediator.Send(new LogoutCommand(usuarioId.Value), ct);
        Response.Cookies.Delete("refreshToken");
        return NoContent();
    }

    private Guid? ObterUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
