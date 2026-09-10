using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class ResumoMensalViewModel(IMediator mediator, UsuarioContexto usuario, PdfService pdfService) : ObservableObject
{
    private static readonly System.Globalization.CultureInfo PtBr = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");

    private static readonly string[] Meses =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    [ObservableProperty]
    private int ano = DateTime.Now.Year;

    [ObservableProperty]
    private int mes = DateTime.Now.Month;

    [ObservableProperty]
    private string labelPeriodo = string.Empty;

    [ObservableProperty]
    private bool carregando;

    [ObservableProperty]
    private ResumoMensalDto? resumo;

    public decimal TotalSaidas =>
        (Resumo?.TotalDespesasFixas ?? 0m) + (Resumo?.TotalDespesasExtras ?? 0m);

    public decimal TotalAQuitar => Resumo is null ? 0m :
        (Resumo.TotalDespesasFixas + Resumo.TotalDespesasExtras)
        - Resumo.DespesasFixas.Where(d => d.Paga).Sum(d => d.Valor)
        - Resumo.DespesasExtras.Where(d => d.Paga).Sum(d => d.Valor);

    partial void OnResumoChanged(ResumoMensalDto? value)
    {
        OnPropertyChanged(nameof(TotalSaidas));
        OnPropertyChanged(nameof(TotalAQuitar));
    }

    [ObservableProperty] private bool expandFixas = true;
    [ObservableProperty] private bool expandExtras = true;
    [ObservableProperty] private bool expandReceitas = true;
    [ObservableProperty] private bool expandReceber = true;

    [RelayCommand] private void ToggleFixas() => ExpandFixas = !ExpandFixas;
    [RelayCommand] private void ToggleExtras() => ExpandExtras = !ExpandExtras;
    [RelayCommand] private void ToggleReceitas() => ExpandReceitas = !ExpandReceitas;
    [RelayCommand] private void ToggleReceber() => ExpandReceber = !ExpandReceber;

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            LabelPeriodo = $"{Meses[Mes - 1]}/{Ano}";
            Resumo = await mediator.Send(new ObterResumoMensalQuery(usuario.UsuarioId, Ano, Mes));
        }
        finally { Carregando = false; }
    }

    [RelayCommand]
    private async Task MesAnteriorAsync()
    {
        Mes--;
        if (Mes < 1) { Mes = 12; Ano--; }
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task MesSeguinteAsync()
    {
        Mes++;
        if (Mes > 12) { Mes = 1; Ano++; }
        await CarregarAsync();
    }

    [ObservableProperty] private bool exportando;

    [RelayCommand]
    private async Task ExportarPdfAsync()
    {
        if (Resumo is null) return;
        Exportando = true;
        try
        {
            List<string[]> linhas =
            [
                ["Receitas", Resumo.TotalReceitas.ToString("C2", PtBr)],
                ["Despesas Fixas", Resumo.TotalDespesasFixas.ToString("C2", PtBr)],
                ["Despesas Extras", Resumo.TotalDespesasExtras.ToString("C2", PtBr)],
                ["Total Saídas", TotalSaidas.ToString("C2", PtBr)],
                ["A Quitar", TotalAQuitar.ToString("C2", PtBr)],
                ["Contas a Receber", Resumo.TotalContasAReceber.ToString("C2", PtBr)],
                ["Saldo do Mês", Resumo.Saldo.ToString("C2", PtBr)],
            ];

            await pdfService.GerarESalvarAsync(
                titulo: "Resumo Mensal",
                periodo: LabelPeriodo,
                corHex: "#4F46E5",
                colunas: ["Indicador", "Valor"],
                linhas: linhas,
                nomeArquivo: $"resumo_mensal_{Ano}_{Mes:D2}.pdf");
        }
        catch { await Shell.Current.DisplayAlert("Erro", "Não foi possível gerar o PDF.", "OK"); }
        finally { Exportando = false; }
    }
}
