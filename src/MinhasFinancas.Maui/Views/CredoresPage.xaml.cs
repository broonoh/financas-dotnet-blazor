using MinhasFinancas.Maui.ViewModels;

namespace MinhasFinancas.Maui.Views;

public partial class CredoresPage : ContentPage
{
    private readonly CredoresViewModel _viewModel;

    public CredoresPage(CredoresViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarCommand.ExecuteAsync(null);
    }
}
