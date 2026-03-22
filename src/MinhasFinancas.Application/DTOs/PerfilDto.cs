namespace MinhasFinancas.Application.DTOs;
public record PerfilDto(Guid Id, string Nome, string Email, DateOnly DataNascimento, string? Telefone, DateTime DataCadastro);
