using MinhasFinancas.Maui.ViewModels;

namespace MinhasFinancas.Maui.Views;

public partial class GraficosPage : ContentPage
{
    private readonly GraficosViewModel _viewModel;

    public GraficosPage(GraficosViewModel viewModel)
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
