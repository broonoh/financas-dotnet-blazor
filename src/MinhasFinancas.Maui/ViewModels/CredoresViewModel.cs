using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.Credores;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class CredoresViewModel(IMediator mediator, UsuarioContexto usuario) : ObservableObject
{
    private Guid? _idEmEdicao;

    public ObservableCollection<CredorDto> Itens { get; } = [];

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string tituloFormulario = "Novo Credor";

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
            var itens = await mediator.Send(new ListarCredoresQuery(usuario.UsuarioId));
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
                await mediator.Send(new AtualizarCredorCommand(id, usuario.UsuarioId, Nome.Trim()));
            else
                await mediator.Send(new CriarCredorCommand(usuario.UsuarioId, Nome.Trim()));

            LimparFormulario();
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void Editar(CredorDto item)
    {
        _idEmEdicao = item.Id;
        Nome = item.Nome;
        TituloFormulario = "Editar Credor";
        EmEdicao = true;
    }

    [RelayCommand]
    private void CancelarEdicao() => LimparFormulario();

    [RelayCommand]
    private async Task ExcluirAsync(CredorDto item)
    {
        var confirmar = await Shell.Current.DisplayAlert("Excluir", $"Excluir o credor \"{item.Nome}\"?", "Sim", "Não");
        if (!confirmar) return;

        try
        {
            await mediator.Send(new ExcluirCredorCommand(item.Id, usuario.UsuarioId));
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
        TituloFormulario = "Novo Credor";
        EmEdicao = false;
    }
}
