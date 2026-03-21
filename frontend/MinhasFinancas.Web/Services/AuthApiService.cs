using System.Net.Http.Json;
using MinhasFinancas.Web.Models;

namespace MinhasFinancas.Web.Services;

public class AuthApiService
{
    private readonly HttpClient _http;

    public AuthApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string senha)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, senha));
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>();
    }

    public async Task LogoutAsync()
    {
        await _http.PostAsync("api/auth/logout", null);
    }

    public async Task<(bool Sucesso, string? Erro)> CadastrarAsync(object payload)
    {
        var response = await _http.PostAsJsonAsync("api/auth/cadastrar", payload);
        if (response.IsSuccessStatusCode) return (true, null);

        try
        {
            var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            var mensagem = body?.Errors != null
                ? string.Join(" | ", body.Errors)
                : body?.Message ?? "Erro ao criar conta.";
            return (false, mensagem);
        }
        catch
        {
            return (false, "Erro ao criar conta. Verifique os dados.");
        }
    }

    private record ErrorResponse(IEnumerable<string>? Errors, string? Message);

    public async Task<bool> RefreshAsync(Guid usuarioId, string refreshToken)
    {
        var response = await _http.PostAsJsonAsync("api/auth/refresh", new { UsuarioId = usuarioId, RefreshToken = refreshToken });
        return response.IsSuccessStatusCode;
    }
}
