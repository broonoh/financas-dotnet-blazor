using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace MinhasFinancas.Web.Services;

/// <summary>
/// DelegatingHandler que injeta o Bearer token JWT em cada requisição HTTP autenticada.
/// Faz logout automático quando a API retorna 401.
/// </summary>
public class AuthenticatedHttpHandler : DelegatingHandler
{
    private readonly AuthStateProvider _authState;
    private readonly NavigationManager _navigation;

    public AuthenticatedHttpHandler(AuthStateProvider authState, NavigationManager navigation)
    {
        _authState = authState;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authState.ObterTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _authState.MarcarComoDesautenticado();
            _navigation.NavigateTo("/login", forceLoad: false);
        }

        return response;
    }
}
