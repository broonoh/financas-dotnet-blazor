using Fluxor;
using MinhasFinancas.Web.Models;
using MinhasFinancas.Web.Services;

namespace MinhasFinancas.Web.Store;

// ==== STATE ====
public record AuthState(bool IsAuthenticated, string? NomeUsuario, Guid? UsuarioId, bool IsLoading, string? Error);

public class AuthFeature : Feature<AuthState>
{
    public override string GetName() => "Auth";
    protected override AuthState GetInitialState() => new(false, null, null, false, null);
}

// ==== ACTIONS ====
public record LoginAction(string Email, string Senha);
public record LoginSuccessAction(string Token, string NomeUsuario, Guid UsuarioId);
public record LoginFailureAction(string Error);
public record LogoutAction;

// ==== REDUCERS ====
public static class AuthReducers
{
    [ReducerMethod]
    public static AuthState OnLogin(AuthState state, LoginAction _)
        => state with { IsLoading = true, Error = null };

    [ReducerMethod]
    public static AuthState OnLoginSuccess(AuthState state, LoginSuccessAction action)
        => state with { IsAuthenticated = true, NomeUsuario = action.NomeUsuario, UsuarioId = action.UsuarioId, IsLoading = false, Error = null };

    [ReducerMethod]
    public static AuthState OnLoginFailure(AuthState state, LoginFailureAction action)
        => state with { IsLoading = false, Error = action.Error };

    [ReducerMethod]
    public static AuthState OnLogout(AuthState state, LogoutAction _)
        => state with { IsAuthenticated = false, NomeUsuario = null, UsuarioId = null };
}

// ==== EFFECTS ====
public class AuthEffects
{
    private readonly AuthApiService _authApi;
    private readonly AuthStateProvider _authState;

    public AuthEffects(AuthApiService authApi, AuthStateProvider authState)
    {
        _authApi = authApi;
        _authState = authState;
    }

    [EffectMethod]
    public async Task HandleLogin(LoginAction action, IDispatcher dispatcher)
    {
        try
        {
            var resultado = await _authApi.LoginAsync(action.Email, action.Senha);
            if (resultado is null)
            {
                dispatcher.Dispatch(new LoginFailureAction("Email ou senha inválidos."));
                return;
            }

            await _authState.MarcarComoAutenticado(resultado.AccessToken, resultado.NomeUsuario, resultado.UsuarioId);
            dispatcher.Dispatch(new LoginSuccessAction(resultado.AccessToken, resultado.NomeUsuario, resultado.UsuarioId));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoginFailureAction($"Erro de conexão: {ex.Message}"));
        }
    }

    [EffectMethod]
    public async Task HandleLogout(LogoutAction _, IDispatcher dispatcher)
    {
        await _authApi.LogoutAsync();
        await _authState.MarcarComoDesautenticado();
    }
}
