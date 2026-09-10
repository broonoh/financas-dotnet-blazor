using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class GraficosViewModel(IMediator mediator, UsuarioContexto usuario) : ObservableObject
{
    private static readonly string[] Meses =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];

    [ObservableProperty] private int mes = DateTime.Now.Month;
    [ObservableProperty] private int ano = DateTime.Now.Year;
    [ObservableProperty] private string labelPeriodo = string.Empty;
    [ObservableProperty] private bool carregando;

    [ObservableProperty] private decimal totalReceitas;
    [ObservableProperty] private decimal totalDespesasFixas;
    [ObservableProperty] private decimal totalDespesasExtras;
    [ObservableProperty] private decimal totalDespesas;
    [ObservableProperty] private decimal saldo;
    [ObservableProperty] private int qtdReceitas;
    [ObservableProperty] private int qtdFixas;
    [ObservableProperty] private int qtdExtras;

    // Larguras já em "pixels" (0-260) para desenhar as barras proporcionais
    [ObservableProperty] private double larguraReceitas;
    [ObservableProperty] private double larguraFixas;
    [ObservableProperty] private double larguraExtras;
    [ObservableProperty] private string pctFixasTexto = "—";
    [ObservableProperty] private string pctExtrasTexto = "—";

    [ObservableProperty] private bool semDevedores = true;

    public ObservableCollection<DevedorGrafico> Devedores { get; } = [];

    public record DevedorGrafico(string Nome, decimal Total, decimal Saldo, double PctRecebido, bool Quitado);

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            LabelPeriodo = $"{Meses[Mes - 1]}/{Ano}";
            var resumo = await mediator.Send(new ObterResumoMensalQuery(usuario.UsuarioId, Ano, Mes));
            ProcessarResumo(resumo);
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

    private void ProcessarResumo(ResumoMensalDto resumo)
    {
        TotalReceitas = resumo.TotalReceitas;
        TotalDespesasFixas = resumo.TotalDespesasFixas;
        TotalDespesasExtras = resumo.TotalDespesasExtras;
        TotalDespesas = resumo.TotalDespesasFixas + resumo.TotalDespesasExtras;
        Saldo = resumo.Saldo;
        QtdReceitas = resumo.Receitas.Count;
        QtdFixas = resumo.DespesasFixas.Count;
        QtdExtras = resumo.DespesasExtras.Count;

        var maior = new[] { TotalReceitas, TotalDespesasFixas, TotalDespesasExtras }.DefaultIfEmpty(0m).Max();
        const double trackWidth = 260;
        LarguraReceitas = maior > 0 ? (double)(TotalReceitas / maior) * trackWidth : 0;
        LarguraFixas = maior > 0 ? (double)(TotalDespesasFixas / maior) * trackWidth : 0;
        LarguraExtras = maior > 0 ? (double)(TotalDespesasExtras / maior) * trackWidth : 0;

        PctFixasTexto = TotalDespesas > 0 ? $"{(double)(TotalDespesasFixas / TotalDespesas) * 100:F1}% das despesas" : "—";
        PctExtrasTexto = TotalDespesas > 0 ? $"{(double)(TotalDespesasExtras / TotalDespesas) * 100:F1}% das despesas" : "—";

        var devedores = resumo.ContasAReceber
            .Select(d =>
            {
                var saldoAberto = d.Parcelas.Where(p => !p.Paga).Sum(p => p.Valor);
                var pago = d.Total - saldoAberto;
                var pct = d.Total > 0 ? (double)(pago / d.Total) : 0;
                return new DevedorGrafico(d.NomeDevedor, d.Total, saldoAberto, pct, saldoAberto <= 0);
            })
            .OrderByDescending(d => d.Total)
            .ToList();

        Devedores.Clear();
        foreach (var d in devedores) Devedores.Add(d);
        SemDevedores = Devedores.Count == 0;
    }
}
