namespace MinhasFinancas.Maui.Services;

public static class AppExitService
{
    public static async Task SairComConfirmacaoAsync()
    {
        var confirmar = await Shell.Current.DisplayAlert("Sair", "Deseja fechar o aplicativo?", "Sim", "Não");
        if (!confirmar) return;

        Environment.Exit(0);
    }
}
