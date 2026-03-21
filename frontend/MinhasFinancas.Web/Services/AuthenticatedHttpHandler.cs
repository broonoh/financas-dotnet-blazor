using Microsoft.AspNetCore.Components.Authorization;

namespace MinhasFinancas.Web.Services;

/// <summary>
/// DelegatingHandler que injeta o Bearer token JWT em cada requisição HTTP autenticada.
/// </summary>
public class AuthenticatedHttpHandler : DelegatingHandler
{
    private readonly AuthStateProvider _authState;

    public AuthenticatedHttpHandler(AuthStateProvider authState)
    {
        _authState = authState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authState.ObterTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
