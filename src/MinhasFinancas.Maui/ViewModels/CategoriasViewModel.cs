using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.Categorias;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class CategoriasViewModel(IMediator mediator, UsuarioContexto usuario) : ObservableObject
{
    private Guid? _idEmEdicao;

    public ObservableCollection<CategoriaDto> Itens { get; } = [];

    [ObservableProperty]
    private bool abaReceita = true;

    public bool AbaDespesa => !AbaReceita;

    partial void OnAbaReceitaChanged(bool value) => OnPropertyChanged(nameof(AbaDespesa));

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string tituloFormulario = "Nova Categoria";

    [ObservableProperty]
    private bool emEdicao;

    [ObservableProperty]
    private bool carregando;

    [RelayCommand]
    private void SelecionarAba(string aba)
    {
        AbaReceita = aba == "receita";
        LimparFormulario();
        _ = CarregarAsync();
    }

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            var itens = AbaReceita
                ? await mediator.Send(new ListarCategoriasReceitaQuery(usuario.UsuarioId))
                : await mediator.Send(new ListarCategoriasDespesaQuery(usuario.UsuarioId));

            Itens.Clear();
            foreach (var item in itens) Itens.Add(item);
        }
        finally { Carregando = false; }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome)) return;
        try
        {
            if (_idEmEdicao is Guid id)
            {
                if (AbaReceita)
                    await mediator.Send(new AtualizarCategoriaReceitaCommand(id, usuario.UsuarioId, Nome.Trim()));
                else
                    await mediator.Send(new AtualizarCategoriaDespesaCommand(id, usuario.UsuarioId, Nome.Trim()));
            }
            else
            {
                if (AbaReceita)
                    await mediator.Send(new CriarCategoriaReceitaCommand(usuario.UsuarioId, Nome.Trim()));
                else
                    await mediator.Send(new CriarCategoriaDespesaCommand(usuario.UsuarioId, Nome.Trim()));
            }

            LimparFormulario();
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void Editar(CategoriaDto item)
    {
        _idEmEdicao = item.Id;
        Nome = item.Nome;
        TituloFormulario = "Editar Categoria";
        EmEdicao = true;
    }

    [RelayCommand]
    private void CancelarEdicao() => LimparFormulario();

    [RelayCommand]
    private async Task ExcluirAsync(CategoriaDto item)
    {
        var confirmar = await Shell.Current.DisplayAlert("Excluir", $"Excluir a categoria \"{item.Nome}\"?", "Sim", "Não");
        if (!confirmar) return;

        if (AbaReceita)
            await mediator.Send(new ExcluirCategoriaReceitaCommand(item.Id, usuario.UsuarioId));
        else
            await mediator.Send(new ExcluirCategoriaDespesaCommand(item.Id, usuario.UsuarioId));

        await CarregarAsync();
    }

    private void LimparFormulario()
    {
        _idEmEdicao = null;
        Nome = string.Empty;
        TituloFormulario = "Nova Categoria";
        EmEdicao = false;
    }
}
