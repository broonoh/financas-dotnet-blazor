using MinhasFinancas.Maui.ViewModels;

namespace MinhasFinancas.Maui.Views;

public partial class FormasPagamentoPage : ContentPage
{
    private readonly FormasPagamentoViewModel _viewModel;

    public FormasPagamentoPage(FormasPagamentoViewModel viewModel)
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
