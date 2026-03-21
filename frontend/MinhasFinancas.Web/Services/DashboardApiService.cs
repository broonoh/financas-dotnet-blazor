using System.Net.Http.Json;
using MinhasFinancas.Web.Models;

namespace MinhasFinancas.Web.Services;

public class DashboardApiService
{
    private readonly HttpClient _http;

    public DashboardApiService(HttpClient http)
    {
        _http = http;
    }

    public Task<DashboardDto?> ObterDashboardAsync(int ano, int mes)
        => _http.GetFromJsonAsync<DashboardDto>($"api/dashboard?ano={ano}&mes={mes}");

    public Task<List<ParcelaDto>?> ListarParcelasAsync(int ano, int mes)
        => _http.GetFromJsonAsync<List<ParcelaDto>>($"api/dashboard/parcelas?ano={ano}&mes={mes}");

    public async Task MarcarParcelaPagaAsync(Guid parcelaId, bool paga)
    {
        await _http.PatchAsJsonAsync(
            $"api/despesas/parcelas/{parcelaId}/pagar",
            new { Paga = paga, DataPagamento = (DateOnly?)null });
    }

    public Task<List<ReceitaDto>?> ListarReceitasAsync()
        => _http.GetFromJsonAsync<List<ReceitaDto>>("api/receitas");

    public async Task<(bool Sucesso, string? Erro)> CriarReceitaAsync(object payload)
    {
        var response = await _http.PostAsJsonAsync("api/receitas", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiErro>();
            return (false, body?.Errors != null ? string.Join(" | ", body.Errors) : body?.Message ?? "Erro ao salvar.");
        }
        catch { return (false, "Erro ao salvar receita."); }
    }

    public async Task<(bool Sucesso, string? Erro)> CriarDespesaFixaAsync(object payload)
    {
        var response = await _http.PostAsJsonAsync("api/despesas/fixas", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiErro>();
            return (false, body?.Errors != null ? string.Join(" | ", body.Errors) : body?.Message ?? "Erro ao salvar.");
        }
        catch { return (false, "Erro ao salvar despesa."); }
    }

    public async Task<(bool Sucesso, string? Erro)> CriarDespesaExtraAsync(object payload)
    {
        var response = await _http.PostAsJsonAsync("api/despesas/extras", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiErro>();
            return (false, body?.Errors != null ? string.Join(" | ", body.Errors) : body?.Message ?? "Erro ao salvar.");
        }
        catch { return (false, "Erro ao salvar despesa."); }
    }

    public async Task<(bool Sucesso, string? Erro)> ExcluirReceitaAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/receitas/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, "Erro ao excluir receita.");
    }

    public Task<List<DespesaFixaListDto>?> ListarDespesasFixasAsync()
        => _http.GetFromJsonAsync<List<DespesaFixaListDto>>("api/despesas/fixas");

    public Task<List<DespesaExtraListDto>?> ListarDespesasExtrasAsync()
        => _http.GetFromJsonAsync<List<DespesaExtraListDto>>("api/despesas/extras");

    public async Task<(bool Sucesso, string? Erro)> ExcluirDespesaFixaAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/despesas/fixas/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, "Erro ao excluir despesa.");
    }

    public async Task<(bool Sucesso, string? Erro)> ExcluirDespesaExtraAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/despesas/extras/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, "Erro ao excluir despesa.");
    }

    public Task<List<DividaDto>?> ListarDividasAsync()
        => _http.GetFromJsonAsync<List<DividaDto>>("api/dividas");

    public async Task<(bool Sucesso, string? Erro)> CriarDividaAsync(object payload)
    {
        var response = await _http.PostAsJsonAsync("api/dividas", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        try
        {
            var body = await response.Content.ReadFromJsonAsync<ApiErro>();
            return (false, body?.Errors != null ? string.Join(" | ", body.Errors) : body?.Message ?? "Erro ao salvar.");
        }
        catch { return (false, "Erro ao salvar dívida."); }
    }

    public async Task<(bool Sucesso, string? Erro)> ExcluirDividaAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/dividas/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, "Erro ao excluir dívida.");
    }

    public async Task MarcarParcelaDividaPagaAsync(Guid parcelaId, bool paga)
    {
        await _http.PatchAsJsonAsync(
            $"api/dividas/parcelas/{parcelaId}/pagar",
            new { Paga = paga, DataPagamento = (DateOnly?)null });
    }

    private record ApiErro(IEnumerable<string>? Errors, string? Message);
}
