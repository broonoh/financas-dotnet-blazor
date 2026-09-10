// O projeto referencia MinhasFinancas.Application (camada de aplicação/MediatR), cujo
// namespace colide com Microsoft.Maui.Controls.Application (classe base do app MAUI).
// Este alias garante que "Application" sem qualificação sempre resolva para a classe MAUI.
global using Application = Microsoft.Maui.Controls.Application;
