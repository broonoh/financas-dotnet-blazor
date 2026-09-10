namespace MinhasFinancas.Maui;

public partial class App : Microsoft.Maui.Controls.Application
{
	public App()
	{
		InitializeComponent();

		// O app não tem um tema escuro desenhado (cores fixas nas telas);
		// força claro para evitar texto branco sobre fundo branco no modo
		// escuro do sistema.
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}