using Fluxor;
using MinhasFinancas.Web.Models;
using MinhasFinancas.Web.Services;

namespace MinhasFinancas.Web.Store;

// ==== STATE ====
public record DashboardState(
    DashboardDto? Dashboard,
    List<ParcelaDto>? Parcelas,
    bool IsLoading,
    string? Error,
    int Ano,
    int Mes);

public class DashboardFeature : Feature<DashboardState>
{
    public override string GetName() => "Dashboard";
    protected override DashboardState GetInitialState()
        => new(null, null, false, null, DateTime.Now.Year, DateTime.Now.Month);
}

// ==== ACTIONS ====
public record CarregarDashboardAction(int Ano, int Mes);
public record DashboardCarregadoAction(DashboardDto Dashboard, List<ParcelaDto> Parcelas);
public record DashboardErroAction(string Error);
public record MarcarParcelaPagaAction(Guid ParcelaId, bool Paga);

// ==== REDUCERS ====
public static class DashboardReducers
{
    [ReducerMethod]
    public static DashboardState OnCarregar(DashboardState state, CarregarDashboardAction action)
        => state with { IsLoading = true, Error = null, Ano = action.Ano, Mes = action.Mes };

    [ReducerMethod]
    public static DashboardState OnCarregado(DashboardState state, DashboardCarregadoAction action)
        => state with { Dashboard = action.Dashboard, Parcelas = action.Parcelas, IsLoading = false };

    [ReducerMethod]
    public static DashboardState OnErro(DashboardState state, DashboardErroAction action)
        => state with { IsLoading = false, Error = action.Error };
}

// ==== EFFECTS ====
public class DashboardEffects
{
    private readonly DashboardApiService _dashboardApi;

    public DashboardEffects(DashboardApiService dashboardApi)
    {
        _dashboardApi = dashboardApi;
    }

    [EffectMethod]
    public async Task HandleCarregar(CarregarDashboardAction action, IDispatcher dispatcher)
    {
        try
        {
            var dashboard = await _dashboardApi.ObterDashboardAsync(action.Ano, action.Mes);
            var parcelas = await _dashboardApi.ListarParcelasAsync(action.Ano, action.Mes);

            if (dashboard is null)
            {
                dispatcher.Dispatch(new DashboardErroAction("Erro ao carregar dados."));
                return;
            }

            dispatcher.Dispatch(new DashboardCarregadoAction(dashboard, parcelas ?? []));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new DashboardErroAction(ex.Message));
        }
    }

    [EffectMethod]
    public async Task HandleMarcarParcela(MarcarParcelaPagaAction action, IDispatcher dispatcher)
    {
        await _dashboardApi.MarcarParcelaPagaAsync(action.ParcelaId, action.Paga);
    }
}
