using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponseDto>
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public RefreshTokenCommandHandler(
        IUsuarioRepository usuarioRepo,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _usuarioRepo = usuarioRepo;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<LoginResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepo.ObterPorIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Usuário não encontrado.");

        if (!usuario.RefreshTokenValido(request.RefreshToken))
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        var accessToken = _tokenService.GerarAccessToken(usuario);
        var novoRefreshToken = _tokenService.GerarRefreshToken();

        usuario.AtualizarRefreshToken(novoRefreshToken, DateTime.UtcNow.AddDays(7));
        _usuarioRepo.Atualizar(usuario);
        await _uow.CommitAsync(cancellationToken);

        return new LoginResponseDto(accessToken, usuario.Nome, usuario.Id);
    }
}
