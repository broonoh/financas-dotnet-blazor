using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Data.Sqlite;
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

    [ObservableProperty]
    private bool fazendoBackup;

    [ObservableProperty]
    private bool restaurando;

    /// <summary>
    /// Gera uma cópia do banco local usando a API nativa de backup do SQLite (segura mesmo
    /// com o banco em uso pelo restante do app) e deixa o usuário escolher onde salvá-la —
    /// inclusive o Google Drive, pelo seletor nativo do Android.
    /// </summary>
    [RelayCommand]
    private async Task FazerBackupAsync()
    {
        FazendoBackup = true;
        var tempPath = Path.Combine(FileSystem.CacheDirectory, $"backup_temp_{Guid.NewGuid():N}.db3");
        try
        {
            using (var origem = new SqliteConnection($"Data Source={LocalBootstrap.CaminhoBanco}"))
            using (var destino = new SqliteConnection($"Data Source={tempPath}"))
            {
                origem.Open();
                destino.Open();
                origem.BackupDatabase(destino);
            }

            using var stream = File.OpenRead(tempPath);
            var nomeArquivo = $"minhas_financas_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db3";
            var resultado = await FileSaver.Default.SaveAsync(nomeArquivo, stream, CancellationToken.None);

            if (resultado.IsSuccessful)
                await Shell.Current.DisplayAlert("Backup concluído",
                    "Backup salvo com sucesso. Escolha o Google Drive ao salvar para manter uma cópia protegida caso o aparelho seja perdido, roubado ou quebre.", "OK");
            else if (resultado.Exception is not null)
                await Shell.Current.DisplayAlert("Erro", $"Não foi possível salvar o backup: {resultado.Exception.Message}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Não foi possível gerar o backup: {ex.Message}", "OK");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
            FazendoBackup = false;
        }
    }

    /// <summary>
    /// Restaura o banco local a partir de um arquivo de backup escolhido pelo usuário
    /// (ex.: salvo anteriormente no Google Drive e baixado para o aparelho).
    /// </summary>
    [RelayCommand]
    private async Task RestaurarBackupAsync()
    {
        FileResult? arquivo;
        try
        {
            arquivo = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Selecione o arquivo de backup (.db3)" });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Não foi possível abrir o seletor de arquivos: {ex.Message}", "OK");
            return;
        }
        if (arquivo is null) return;

        var confirmar = await Shell.Current.DisplayAlert(
            "Restaurar backup",
            "Isso vai substituir TODOS os dados atuais do aplicativo pelos dados do arquivo selecionado. Esta ação não pode ser desfeita. Deseja continuar?",
            "Sim, restaurar", "Cancelar");
        if (!confirmar) return;

        Restaurando = true;
        var tempPath = Path.Combine(FileSystem.CacheDirectory, $"restore_temp_{Guid.NewGuid():N}.db3");
        try
        {
            await using (var origemStream = await arquivo.OpenReadAsync())
            await using (var tempFile = File.Create(tempPath))
                await origemStream.CopyToAsync(tempFile);

            using (var origem = new SqliteConnection($"Data Source={tempPath}"))
            using (var destino = new SqliteConnection($"Data Source={LocalBootstrap.CaminhoBanco}"))
            {
                origem.Open();
                destino.Open();
                origem.BackupDatabase(destino);
            }

            await Shell.Current.DisplayAlert("Backup restaurado",
                "Os dados foram restaurados. Feche e abra o aplicativo novamente para ver as informações atualizadas.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Erro", $"Não foi possível restaurar o backup: {ex.Message}", "OK");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
            Restaurando = false;
        }
    }
}
