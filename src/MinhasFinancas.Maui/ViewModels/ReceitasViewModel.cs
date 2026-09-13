using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.Receitas;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class ReceitasViewModel(IMediator mediator, UsuarioContexto usuario, PdfService pdfService) : ObservableObject
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private Guid? _idEmEdicao;

    public ObservableCollection<ReceitaDto> Itens { get; } = [];
    public ObservableCollection<CategoriaDto> Categorias { get; } = [];

    [ObservableProperty]
    private string descricao = string.Empty;

    [ObservableProperty]
    private string valorStr = string.Empty;

    [ObservableProperty]
    private DateTime dataRecebimento = DateTime.Now;

    [ObservableProperty]
    private CategoriaDto? categoriaSelecionada;

    [ObservableProperty]
    private bool registrarParaProximoMes;

    [ObservableProperty]
    private string tituloFormulario = "Nova Receita";

    [ObservableProperty]
    private bool emEdicao;

    [ObservableProperty]
    private bool carregando;

    [ObservableProperty]
    private decimal totalGeral;

    [ObservableProperty]
    private bool exportando;

    [ObservableProperty]
    private int mesFiltro = DateTime.Now.Month;

    [ObservableProperty]
    private int anoFiltro = DateTime.Now.Year;

    [RelayCommand]
    private async Task PeriodoAnteriorAsync()
    {
        MesFiltro--;
        if (MesFiltro < 1) { MesFiltro = 12; AnoFiltro--; }
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task PeriodoSeguinteAsync()
    {
        MesFiltro++;
        if (MesFiltro > 12) { MesFiltro = 1; AnoFiltro++; }
        await CarregarAsync();
    }

    [RelayCommand]
    private async Task ExportarPdfAsync()
    {
        Exportando = true;
        try
        {
            var linhas = Itens
                .OrderByDescending(r => r.DataRecebimento)
                .Select(r => new[]
                {
                    r.Descricao,
                    r.Categoria,
                    r.DataRecebimento.ToString("dd/MM/yyyy"),
                    EhProximoMes(r) ? "Próximo Mês" : "Mês Atual",
                    r.Valor.ToString("C2", PtBr)
                })
                .ToList();

            await pdfService.GerarESalvarAsync(
                titulo: "Receitas",
                periodo: $"{Itens.Count} lançamento(s)",
                corHex: "#2E7D32",
                colunas: ["Descrição", "Categoria", "Data", "Mês Ref.", "Valor"],
                linhas: linhas,
                nomeArquivo: $"receitas_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                linhaTotal: ["", "", "", "TOTAL", TotalGeral.ToString("C2", PtBr)]);
        }
        catch { await Shell.Current.DisplayAlert("Erro", "Não foi possível gerar o PDF.", "OK"); }
        finally { Exportando = false; }
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            var categorias = await mediator.Send(new ListarCategoriasReceitaQuery(usuario.UsuarioId));
            Categorias.Clear();
            foreach (var c in categorias) Categorias.Add(c);

            var itens = await mediator.Send(new ListarReceitasQuery(usuario.UsuarioId, AnoFiltro, MesFiltro));
            Itens.Clear();
            foreach (var item in itens.OrderByDescending(r => r.DataRecebimento)) Itens.Add(item);
            TotalGeral = Itens.Sum(i => i.Valor);
        }
        finally { Carregando = false; }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Descricao) || CategoriaSelecionada is null)
        {
            await Shell.Current.DisplayAlert("Erro", "Preencha descrição e categoria.", "OK");
            return;
        }
        if (!decimal.TryParse(ValorStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var valor) || valor <= 0)
        {
            await Shell.Current.DisplayAlert("Erro", "Valor inválido.", "OK");
            return;
        }

        try
        {
            var dataOnly = DateOnly.FromDateTime(DataRecebimento);
            if (_idEmEdicao is Guid id)
                await mediator.Send(new AtualizarReceitaCommand(id, usuario.UsuarioId, Descricao.Trim(), valor, dataOnly, CategoriaSelecionada.Nome, RegistrarParaProximoMes));
            else
                await mediator.Send(new CriarReceitaCommand(usuario.UsuarioId, Descricao.Trim(), valor, dataOnly, CategoriaSelecionada.Nome, RegistrarParaProximoMes));

            LimparFormulario();
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or FluentValidation.ValidationException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void Editar(ReceitaDto item)
    {
        _idEmEdicao = item.Id;
        EmEdicao = true;
        Descricao = item.Descricao;
        ValorStr = item.Valor.ToString("F2", CultureInfo.InvariantCulture);
        DataRecebimento = item.DataRecebimento.ToDateTime(TimeOnly.MinValue);
        CategoriaSelecionada = Categorias.FirstOrDefault(c => c.Nome == item.Categoria);
        RegistrarParaProximoMes = EhProximoMes(item);
        TituloFormulario = "Editar Receita";
    }

    private bool _suprimirPerguntaMes;

    partial void OnDataRecebimentoChanged(DateTime value)
    {
        if (_suprimirPerguntaMes) return;
        _ = PerguntarMesReferenciaAsync(value);
    }

    private async Task PerguntarMesReferenciaAsync(DateTime value)
    {
        if (EmEdicao || value.Day <= 25) return;

        var resposta = await Shell.Current.DisplayActionSheet(
            "A receita a ser cadastrada será para o mês atual ou para o próximo mês?",
            null, null, "Mês Atual", "Próximo Mês");
        RegistrarParaProximoMes = resposta == "Próximo Mês";
    }

    private static bool EhProximoMes(ReceitaDto r)
        => r.MesReferencia.Year != r.DataRecebimento.Year || r.MesReferencia.Month != r.DataRecebimento.Month;

    [RelayCommand]
    private void CancelarEdicao() => LimparFormulario();

    [RelayCommand]
    private async Task ExcluirAsync(ReceitaDto item)
    {
        var confirmar = await Shell.Current.DisplayAlert("Excluir", $"Excluir a receita \"{item.Descricao}\"?", "Sim", "Não");
        if (!confirmar) return;

        await mediator.Send(new ExcluirReceitaCommand(item.Id, usuario.UsuarioId));
        await CarregarAsync();
    }

    private void LimparFormulario()
    {
        _idEmEdicao = null;
        Descricao = string.Empty;
        ValorStr = string.Empty;
        _suprimirPerguntaMes = true;
        DataRecebimento = DateTime.Now;
        _suprimirPerguntaMes = false;
        CategoriaSelecionada = Categorias.FirstOrDefault();
        RegistrarParaProximoMes = false;
        TituloFormulario = "Nova Receita";
        EmEdicao = false;
    }
}
