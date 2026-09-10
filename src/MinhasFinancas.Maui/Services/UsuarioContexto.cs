namespace MinhasFinancas.Maui.Services;

/// <summary>
/// App local e single-user: não há login. Guarda o Id do único Usuario
/// cadastrado neste dispositivo, criado automaticamente no primeiro uso.
/// </summary>
public class UsuarioContexto
{
    public Guid UsuarioId { get; set; }
}
