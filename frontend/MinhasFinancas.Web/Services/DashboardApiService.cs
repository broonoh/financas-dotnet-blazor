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
        return (false, await ExtrairErro(response, "Erro ao salvar receita."));
    }

    public async Task<(bool Sucesso, string? Erro)> CriarDespesaFixaAsync(object payload)
    {
        var response = await _http.PostAsJsonAsync("api/despesas/fixas", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao salvar despesa."));
    }

    public async Task<(bool Sucesso, string? Erro)> CriarDespesaExtraAsync(object payload)
    {
        var response = await _http.PostAsJsonAsync("api/despesas/extras", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao salvar despesa."));
    }

    public async Task<(bool Sucesso, string? Erro)> AtualizarReceitaAsync(Guid id, object payload)
    {
        var response = await _http.PutAsJsonAsync($"api/receitas/{id}", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao atualizar receita."));
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

    public async Task<(bool Sucesso, string? Erro)> AtualizarDespesaFixaAsync(Guid id, object payload)
    {
        var response = await _http.PutAsJsonAsync($"api/despesas/fixas/{id}", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao atualizar despesa."));
    }

    public async Task<(bool Sucesso, string? Erro)> AtualizarDespesaExtraAsync(Guid id, object payload)
    {
        var response = await _http.PutAsJsonAsync($"api/despesas/extras/{id}", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao atualizar despesa."));
    }

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
        return (false, await ExtrairErro(response, "Erro ao salvar dívida."));
    }

    public async Task<(bool Sucesso, string? Erro)> AtualizarDividaAsync(Guid id, object payload)
    {
        var response = await _http.PutAsJsonAsync($"api/dividas/{id}", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao atualizar dívida."));
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

    public Task<PerfilDto?> ObterPerfilAsync()
        => _http.GetFromJsonAsync<PerfilDto>("api/perfil");

    public async Task<(bool Sucesso, string? Erro)> AtualizarPerfilAsync(object payload)
    {
        var response = await _http.PutAsJsonAsync("api/perfil", payload);
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao atualizar perfil."));
    }

    public Task<List<CategoriaDto>?> ListarCategoriasReceitaAsync()
        => _http.GetFromJsonAsync<List<CategoriaDto>>("api/categorias/receita");

    public async Task<(bool Sucesso, string? Erro)> CriarCategoriaReceitaAsync(string nome)
    {
        var response = await _http.PostAsJsonAsync("api/categorias/receita", new { Nome = nome });
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao salvar categoria."));
    }

    public async Task<(bool Sucesso, string? Erro)> AtualizarCategoriaReceitaAsync(Guid id, string nome)
    {
        var response = await _http.PutAsJsonAsync($"api/categorias/receita/{id}", new { Nome = nome });
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao atualizar categoria."));
    }

    public async Task<(bool Sucesso, string? Erro)> ExcluirCategoriaReceitaAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/categorias/receita/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, "Erro ao excluir categoria.");
    }

    public Task<List<CategoriaDto>?> ListarCategoriasDespesaAsync()
        => _http.GetFromJsonAsync<List<CategoriaDto>>("api/categorias/despesa");

    public async Task<(bool Sucesso, string? Erro)> CriarCategoriaDespesaAsync(string nome)
    {
        var response = await _http.PostAsJsonAsync("api/categorias/despesa", new { Nome = nome });
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao salvar categoria."));
    }

    public async Task<(bool Sucesso, string? Erro)> AtualizarCategoriaDespesaAsync(Guid id, string nome)
    {
        var response = await _http.PutAsJsonAsync($"api/categorias/despesa/{id}", new { Nome = nome });
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, await ExtrairErro(response, "Erro ao atualizar categoria."));
    }

    public async Task<(bool Sucesso, string? Erro)> ExcluirCategoriaDespesaAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"api/categorias/despesa/{id}");
        if (response.IsSuccessStatusCode) return (true, null);
        return (false, "Erro ao excluir categoria.");
    }

    public async Task<byte[]?> DownloadDividasPdfAsync(string nomeDevedor)
    {
        var response = await _http.GetAsync($"api/dividas/export/pdf?nomeDevedor={Uri.EscapeDataString(nomeDevedor)}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    private static async Task<string> ExtrairErro(HttpResponseMessage response, string fallback)
    {
        var statusCode = (int)response.StatusCode;
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body)) return $"{fallback} (HTTP {statusCode})";

            using var doc = System.Text.Json.JsonDocument.Parse(body,
                new System.Text.Json.JsonDocumentOptions { });
            var root = doc.RootElement;

            // Try finding "errors" case-insensitively
            var errsEl = root.EnumerateObject()
                .FirstOrDefault(p => p.Name.Equals("errors", StringComparison.OrdinalIgnoreCase)).Value;

            if (errsEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                // FluentValidation format: { "errors": ["msg1", "msg2"] }
                return string.Join(" | ", errsEl.EnumerateArray()
                    .Select(e => e.GetString()).Where(s => s != null)!);

            if (errsEl.ValueKind == System.Text.Json.JsonValueKind.Object)
                // ValidationProblemDetails format: { "errors": { "Field": ["msg1"] } }
                return string.Join(" | ", errsEl.EnumerateObject()
                    .SelectMany(p => p.Value.EnumerateArray().Select(e => e.GetString()))
                    .Where(s => s != null)!);

            var message = root.EnumerateObject()
                .FirstOrDefault(p => p.Name.Equals("message", StringComparison.OrdinalIgnoreCase)).Value;
            if (message.ValueKind == System.Text.Json.JsonValueKind.String)
                return message.GetString() ?? fallback;

            var title = root.EnumerateObject()
                .FirstOrDefault(p => p.Name.Equals("title", StringComparison.OrdinalIgnoreCase)).Value;
            if (title.ValueKind == System.Text.Json.JsonValueKind.String)
                return title.GetString() ?? fallback;

            return body.Length <= 300 ? $"HTTP {statusCode}: {body}" : $"{fallback} (HTTP {statusCode})";
        }
        catch
        {
            return $"{fallback} (HTTP {statusCode})";
        }
    }
}
