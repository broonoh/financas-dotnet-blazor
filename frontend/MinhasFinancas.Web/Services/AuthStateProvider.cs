using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace MinhasFinancas.Web.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public AuthStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _js.InvokeAsync<string?>("localStorage.getItem", "accessToken");
            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            if (TokenExpirado(token))
            {
                await MarcarComoDesautenticado();
                return new AuthenticationState(_anonymous);
            }

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    private static bool TokenExpirado(string token)
    {
        try
        {
            var claims = ParseClaimsFromJwt(token);
            var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            if (exp == null) return false;
            var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
            return expDate.UtcDateTime < DateTime.UtcNow;
        }
        catch { return false; }
    }

    public async Task MarcarComoAutenticado(string token, string nome, Guid id)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "accessToken", token);
        await _js.InvokeVoidAsync("localStorage.setItem", "nomeUsuario", nome);
        await _js.InvokeVoidAsync("localStorage.setItem", "usuarioId", id.ToString());

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarcarComoDesautenticado()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "accessToken");
        await _js.InvokeVoidAsync("localStorage.removeItem", "nomeUsuario");
        await _js.InvokeVoidAsync("localStorage.removeItem", "usuarioId");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    public async Task<string?> ObterTokenAsync()
        => await _js.InvokeAsync<string?>("localStorage.getItem", "accessToken");

    public async Task<string?> ObterNomeAsync()
        => await _js.InvokeAsync<string?>("localStorage.getItem", "nomeUsuario");

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
        return keyValuePairs?.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())) ?? [];
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
