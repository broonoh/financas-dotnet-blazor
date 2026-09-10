using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using MinhasFinancas.Application.Commands.Auth;
using MinhasFinancas.Application.Queries;
using MinhasFinancas.Maui.Services;

namespace MinhasFinancas.Maui.ViewModels;

public partial class PerfilViewModel(IMediator mediator, UsuarioContexto usuario) : ObservableObject
{
    [ObservableProperty]
    private string nome = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private DateTime dataNascimento = new(2000, 1, 1);

    [ObservableProperty]
    private string? telefone;

    [ObservableProperty]
    private string dataCadastro = string.Empty;

    [ObservableProperty]
    private bool carregando;

    [ObservableProperty]
    private bool salvando;

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            var perfil = await mediator.Send(new ObterPerfilQuery(usuario.UsuarioId));
            Nome = perfil.Nome;
            Email = perfil.Email;
            DataNascimento = perfil.DataNascimento.ToDateTime(TimeOnly.MinValue);
            Telefone = perfil.Telefone;
            DataCadastro = perfil.DataCadastro.ToLocalTime().ToString("dd/MM/yyyy");
        }
        finally { Carregando = false; }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            await Shell.Current.DisplayAlert("Erro", "Informe seu nome.", "OK");
            return;
        }

        Salvando = true;
        try
        {
            await mediator.Send(new AtualizarPerfilCommand(usuario.UsuarioId, Nome.Trim(), Telefone,
                Email.Trim(), DateOnly.FromDateTime(DataNascimento)));
            await Shell.Current.DisplayAlert("Pronto", "Perfil atualizado.", "OK");
        }
        catch (ArgumentException ex)
        {
            await Shell.Current.DisplayAlert("Erro", ex.Message, "OK");
        }
        finally { Salvando = false; }
    }

    [RelayCommand]
    private async Task SairDoAppAsync() => await AppExitService.SairComConfirmacaoAsync();
}
