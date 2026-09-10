using MinhasFinancas.Maui.ViewModels;

namespace MinhasFinancas.Maui.Views;

public partial class DevedoresPage : ContentPage
{
    private readonly DevedoresViewModel _viewModel;

    public DevedoresPage(DevedoresViewModel viewModel)
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
