using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.Devedores;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class DevedoresViewModel(IMediator mediator, UsuarioContexto usuario) : ObservableObject
{
    private Guid? _idEmEdicao;

    public ObservableCollection<DevedorDto> Itens { get; } = [];

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string tituloFormulario = "Novo Devedor";

    [ObservableProperty]
    private bool emEdicao;

    [ObservableProperty]
    private bool carregando;

    [RelayCommand]
    public async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            var itens = await mediator.Send(new ListarDevedoresQuery(usuario.UsuarioId));
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
                await mediator.Send(new AtualizarDevedorCommand(id, usuario.UsuarioId, Nome.Trim()));
            else
                await mediator.Send(new CriarDevedorCommand(usuario.UsuarioId, Nome.Trim()));

            LimparFormulario();
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void Editar(DevedorDto item)
    {
        _idEmEdicao = item.Id;
        Nome = item.Nome;
        TituloFormulario = "Editar Devedor";
        EmEdicao = true;
    }

    [RelayCommand]
    private void CancelarEdicao() => LimparFormulario();

    [RelayCommand]
    private async Task ExcluirAsync(DevedorDto item)
    {
        var confirmar = await Shell.Current.DisplayAlert("Excluir", $"Excluir o devedor \"{item.Nome}\"?", "Sim", "Não");
        if (!confirmar) return;

        try
        {
            await mediator.Send(new ExcluirDevedorCommand(item.Id, usuario.UsuarioId));
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    private void LimparFormulario()
    {
        _idEmEdicao = null;
        Nome = string.Empty;
        TituloFormulario = "Novo Devedor";
        EmEdicao = false;
    }
}
