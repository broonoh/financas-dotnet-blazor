using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.FormasPagamento;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class FormasPagamentoViewModel(IMediator mediator, UsuarioContexto usuario) : ObservableObject
{
    private Guid? _idEmEdicao;

    public ObservableCollection<FormaPagamentoDto> Itens { get; } = [];

    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string tituloFormulario = "Nova Forma de Pagamento";

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
            var itens = await mediator.Send(new ListarFormasPagamentoQuery(usuario.UsuarioId));
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
                await mediator.Send(new AtualizarFormaPagamentoCommand(id, usuario.UsuarioId, Nome.Trim()));
            else
                await mediator.Send(new CriarFormaPagamentoCommand(usuario.UsuarioId, Nome.Trim()));

            LimparFormulario();
            await CarregarAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void Editar(FormaPagamentoDto item)
    {
        _idEmEdicao = item.Id;
        Nome = item.Nome;
        TituloFormulario = "Editar Forma de Pagamento";
        EmEdicao = true;
    }

    [RelayCommand]
    private void CancelarEdicao() => LimparFormulario();

    [RelayCommand]
    private async Task ExcluirAsync(FormaPagamentoDto item)
    {
        var confirmar = await Shell.Current.DisplayAlert("Excluir", $"Excluir a forma de pagamento \"{item.Nome}\"?", "Sim", "Não");
        if (!confirmar) return;

        try
        {
            await mediator.Send(new ExcluirFormaPagamentoCommand(item.Id, usuario.UsuarioId));
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
        TituloFormulario = "Nova Forma de Pagamento";
        EmEdicao = false;
    }
}
