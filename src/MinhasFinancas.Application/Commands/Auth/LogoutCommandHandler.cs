using MediatR;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Auth;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IUnitOfWork _uow;

    public LogoutCommandHandler(IUsuarioRepository usuarioRepo, IUnitOfWork uow)
    {
        _usuarioRepo = usuarioRepo;
        _uow = uow;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepo.ObterPorIdAsync(request.UsuarioId, cancellationToken);
        if (usuario is null) return;

        usuario.AtualizarRefreshToken(null, null);
        _usuarioRepo.Atualizar(usuario);
        await _uow.CommitAsync(cancellationToken);
    }
}
