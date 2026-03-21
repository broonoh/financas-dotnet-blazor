using System.Collections.Concurrent;

namespace MinhasFinancas.Infrastructure.Services;

/// <summary>
/// Rate limiting em memória para tentativas de login.
/// Singleton: 5 tentativas em 15 minutos, lockout de 30 minutos.
/// </summary>
public class LoginAttemptService
{
    private const int MaxTentativas = 5;
    private static readonly TimeSpan JanelaTentativas = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan TempoBloqueio = TimeSpan.FromMinutes(30);

    private record TentativasInfo(List<DateTime> Tentativas, DateTime? BloqueiAte);

    private readonly ConcurrentDictionary<string, TentativasInfo> _tentativas = new();

    public bool EstaBloqueado(string email)
    {
        if (!_tentativas.TryGetValue(email.ToLowerInvariant(), out var info))
            return false;

        if (info.BloqueiAte.HasValue && info.BloqueiAte.Value > DateTime.UtcNow)
            return true;

        // Limpar bloqueio expirado
        if (info.BloqueiAte.HasValue)
            _tentativas.TryRemove(email.ToLowerInvariant(), out _);

        return false;
    }

    public void RegistrarTentativaFalha(string email)
    {
        var key = email.ToLowerInvariant();
        var agora = DateTime.UtcNow;

        _tentativas.AddOrUpdate(key,
            _ => new TentativasInfo(new List<DateTime> { agora }, null),
            (_, info) =>
            {
                // Remover tentativas fora da janela
                var tentativasRecentes = info.Tentativas
                    .Where(t => t > agora - JanelaTentativas)
                    .Append(agora)
                    .ToList();

                DateTime? bloqueiAte = null;
                if (tentativasRecentes.Count >= MaxTentativas)
                    bloqueiAte = agora.Add(TempoBloqueio);

                return new TentativasInfo(tentativasRecentes, bloqueiAte);
            });
    }

    public void LimparTentativas(string email)
        => _tentativas.TryRemove(email.ToLowerInvariant(), out _);

    public TimeSpan? TempoRestanteBloqueio(string email)
    {
        if (!_tentativas.TryGetValue(email.ToLowerInvariant(), out var info))
            return null;

        if (info.BloqueiAte.HasValue && info.BloqueiAte.Value > DateTime.UtcNow)
            return info.BloqueiAte.Value - DateTime.UtcNow;

        return null;
    }
}
