using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.Despesas;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class ParcelasViewModel(IMediator mediator, UsuarioContexto usuario) : ObservableObject
{
    private static readonly string[] Meses =
    [
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    ];
    public string[] NomesMeses => Meses;
    public int[] AnosOpcoes { get; } = Enumerable.Range(DateTime.Now.Year - 1, 4).ToArray();

    private List<ParcelaDto> _todas = [];

    [ObservableProperty] private int mesSelecionado = DateTime.Now.Month;
    [ObservableProperty] private int anoSelecionado = DateTime.Now.Year;

    public int MesIndex
    {
        get => MesSelecionado - 1;
        set => MesSelecionado = value + 1;
    }

    partial void OnMesSelecionadoChanged(int value) => OnPropertyChanged(nameof(MesIndex));
    [ObservableProperty] private bool carregando;
    [ObservableProperty] private int abaAtiva; // 0=Todas 1=Vencidas 2=Pendentes 3=Pagas

    public ObservableCollection<ParcelaDto> ItensExibidos { get; } = [];

    [ObservableProperty] private int totalCount;
    [ObservableProperty] private decimal totalValor;
    [ObservableProperty] private int pagasCount;
    [ObservableProperty] private decimal pagasValor;
    [ObservableProperty] private int pendentesCount;
    [ObservableProperty] private decimal pendentesValor;
    [ObservableProperty] private int vencidasCount;
    [ObservableProperty] private decimal vencidasValor;

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            var resultado = await mediator.Send(new ListarParcelasMesQuery(usuario.UsuarioId, AnoSelecionado, MesSelecionado));
            _todas = resultado.ToList();

            var pagas = _todas.Where(p => p.Paga).ToList();
            var vencidas = _todas.Where(p => !p.Paga && p.Vencida).ToList();
            var pendentes = _todas.Where(p => !p.Paga).ToList();

            TotalCount = _todas.Count;
            TotalValor = _todas.Sum(p => p.Valor);
            PagasCount = pagas.Count;
            PagasValor = pagas.Sum(p => p.Valor);
            PendentesCount = pendentes.Count;
            PendentesValor = pendentes.Sum(p => p.Valor);
            VencidasCount = vencidas.Count;
            VencidasValor = vencidas.Sum(p => p.Valor);

            AtualizarListaExibida();
        }
        finally { Carregando = false; }
    }

    [RelayCommand]
    private void SelecionarAba(string aba)
    {
        AbaAtiva = aba switch { "vencidas" => 1, "pendentes" => 2, "pagas" => 3, _ => 0 };
        AtualizarListaExibida();
    }

    private void AtualizarListaExibida()
    {
        IEnumerable<ParcelaDto> filtradas = AbaAtiva switch
        {
            1 => _todas.Where(p => !p.Paga && p.Vencida),
            2 => _todas.Where(p => !p.Paga),
            3 => _todas.Where(p => p.Paga),
            _ => _todas
        };

        ItensExibidos.Clear();
        foreach (var p in filtradas.OrderBy(p => p.DataVencimento)) ItensExibidos.Add(p);
    }

    [RelayCommand]
    private async Task ToggleParcelaAsync(ParcelaDto parcela)
    {
        await mediator.Send(new MarcarParcelaPagaCommand(parcela.Id, usuario.UsuarioId, !parcela.Paga));
        await CarregarAsync();
    }
}
