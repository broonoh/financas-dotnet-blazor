using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly ISenhaService _senhaService;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _uow;

    public LoginCommandHandler(
        IUsuarioRepository usuarioRepo,
        ISenhaService senhaService,
        ITokenService tokenService,
        IUnitOfWork uow)
    {
        _usuarioRepo = usuarioRepo;
        _senhaService = senhaService;
        _tokenService = tokenService;
        _uow = uow;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepo.ObterPorEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Conta desativada.");

        if (!_senhaService.VerificarSenha(request.Senha, usuario.SenhaHash.Valor))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var accessToken = _tokenService.GerarAccessToken(usuario);
        var refreshToken = _tokenService.GerarRefreshToken();

        usuario.AtualizarRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        _usuarioRepo.Atualizar(usuario);
        await _uow.CommitAsync(cancellationToken);

        return new LoginResponseDto(accessToken, usuario.Nome, usuario.Id);
    }
}
