namespace MinhasFinancas.Application.DTOs;

public record UsuarioDto(Guid Id, string Nome, string Email);
public record LoginResponseDto(string AccessToken, string NomeUsuario, Guid UsuarioId);
