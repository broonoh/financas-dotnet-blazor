using MinhasFinancas.Maui.ViewModels;

namespace MinhasFinancas.Maui.Views;

public partial class ReceitasPage : ContentPage
{
    private readonly ReceitasViewModel _viewModel;

    public ReceitasPage(ReceitasViewModel viewModel)
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
