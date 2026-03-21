using MediatR;
using MinhasFinancas.Application.DTOs;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Interfaces;

namespace MinhasFinancas.Application.Commands.Auth;

public class CadastrarUsuarioCommandHandler : IRequestHandler<CadastrarUsuarioCommand, UsuarioDto>
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly ISenhaService _senhaService;
    private readonly IUnitOfWork _uow;

    public CadastrarUsuarioCommandHandler(
        IUsuarioRepository usuarioRepo,
        ISenhaService senhaService,
        IUnitOfWork uow)
    {
        _usuarioRepo = usuarioRepo;
        _senhaService = senhaService;
        _uow = uow;
    }

    public async Task<UsuarioDto> Handle(CadastrarUsuarioCommand request, CancellationToken cancellationToken)
    {
        if (await _usuarioRepo.ExisteEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email já cadastrado.");

        var senhaHash = _senhaService.HashSenha(request.Senha);
        var usuario = Usuario.Criar(request.Nome, request.Email, senhaHash, request.DataNascimento, request.Telefone);

        await _usuarioRepo.AdicionarAsync(usuario, cancellationToken);
        await _uow.CommitAsync(cancellationToken);

        return new UsuarioDto(usuario.Id, usuario.Nome, usuario.Email.Valor);
    }
}
