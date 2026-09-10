using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.Despesas;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class DashboardViewModel(IMediator mediator, UsuarioContexto usuario) : ObservableObject
{
    private const double TrackWidth = 140;
    private const double CategoriaTrackWidth = 220;

    [ObservableProperty]
    private DashboardDto? dashboard;

    [ObservableProperty]
    private bool carregando;

    [ObservableProperty] private bool expandEvolucao = true;
    [ObservableProperty] private bool expandComparativo = true;

    [RelayCommand] private void ToggleEvolucao() => ExpandEvolucao = !ExpandEvolucao;
    [RelayCommand] private void ToggleComparativo() => ExpandComparativo = !ExpandComparativo;

    [RelayCommand] private async Task SairDoAppAsync() => await AppExitService.SairComConfirmacaoAsync();

    public ObservableCollection<ParcelaDto> Parcelas { get; } = [];
    public ObservableCollection<MesBarraItem> Evolucao12Meses { get; } = [];
    public ObservableCollection<MesBarraItem> Comparativo6Meses { get; } = [];
    public ObservableCollection<CategoriaBarraItem> DistribuicaoCategorias { get; } = [];

    public record MesBarraItem(string Mes, decimal Receitas, decimal Despesas, decimal Saldo, double LarguraReceitas, double LarguraDespesas);
    public record CategoriaBarraItem(string Categoria, decimal Valor, decimal Percentual, double Largura);

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            var hoje = DateTime.Now;
            Dashboard = await mediator.Send(new ObterDashboardQuery(usuario.UsuarioId, hoje.Year, hoje.Month));

            var parcelas = await mediator.Send(new ListarParcelasMesQuery(usuario.UsuarioId, hoje.Year, hoje.Month));
            Parcelas.Clear();
            foreach (var p in parcelas.OrderBy(p => p.DataVencimento)) Parcelas.Add(p);

            ProcessarGraficos(Dashboard);
        }
        finally { Carregando = false; }
    }

    private void ProcessarGraficos(DashboardDto dash)
    {
        var maiorMensal = dash.Evolucao12Meses.Concat(dash.Comparativo6Meses)
            .SelectMany(m => new[] { m.Receitas, m.Despesas })
            .DefaultIfEmpty(0m).Max();

        Evolucao12Meses.Clear();
        foreach (var m in dash.Evolucao12Meses.Where(m => m.Receitas != 0 || m.Despesas != 0))
            Evolucao12Meses.Add(new MesBarraItem(m.Mes, m.Receitas, m.Despesas, m.Saldo,
                LarguraDe(m.Receitas, maiorMensal, TrackWidth), LarguraDe(m.Despesas, maiorMensal, TrackWidth)));

        Comparativo6Meses.Clear();
        foreach (var m in dash.Comparativo6Meses.Where(m => m.Receitas != 0 || m.Despesas != 0))
            Comparativo6Meses.Add(new MesBarraItem(m.Mes, m.Receitas, m.Despesas, m.Saldo,
                LarguraDe(m.Receitas, maiorMensal, TrackWidth), LarguraDe(m.Despesas, maiorMensal, TrackWidth)));

        var maiorCategoria = dash.DistribuicaoCategorias.Select(c => c.Valor).DefaultIfEmpty(0m).Max();
        DistribuicaoCategorias.Clear();
        foreach (var c in dash.DistribuicaoCategorias.OrderByDescending(c => c.Valor))
            DistribuicaoCategorias.Add(new CategoriaBarraItem(c.Categoria, c.Valor, c.Percentual,
                LarguraDe(c.Valor, maiorCategoria, CategoriaTrackWidth)));
    }

    private static double LarguraDe(decimal valor, decimal maior, double track) =>
        maior > 0 ? (double)(valor / maior) * track : 0;

    [RelayCommand]
    private async Task ToggleParcelaAsync(ParcelaDto parcela)
    {
        await mediator.Send(new MarcarParcelaPagaCommand(parcela.Id, usuario.UsuarioId, !parcela.Paga));
        await CarregarAsync();
    }
}
