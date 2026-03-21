using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Tests.Unit;

public class UsuarioTests
{
    private static DateOnly DataNascimentoAdulto => DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25));
    private static DateOnly DataNascimentoMenor => DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-17));

    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarUsuarioAtivo()
    {
        var usuario = Usuario.Criar(
            "João da Silva",
            "joao@email.com",
            "hash123",
            DataNascimentoAdulto);

        usuario.Id.Should().NotBeEmpty();
        usuario.Nome.Should().Be("João da Silva");
        usuario.Email.Valor.Should().Be("joao@email.com");
        usuario.Ativo.Should().BeTrue();
        usuario.DataCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Criar_ComEmailMaiusculo_DeveNormalizarParaMinusculo()
    {
        var usuario = Usuario.Criar(
            "Maria", "MARIA@EMAIL.COM", "hash", DataNascimentoAdulto);

        usuario.Email.Valor.Should().Be("maria@email.com");
    }

    [Fact]
    public void Criar_ComMenor18Anos_DeveLancarExcecao()
    {
        var act = () => Usuario.Criar(
            "Junior", "junior@email.com", "hash", DataNascimentoMenor);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*18 anos*");
    }

    [Fact]
    public void Criar_ComNomeCurto_DeveLancarExcecao()
    {
        var act = () => Usuario.Criar("AB", "ab@email.com", "hash", DataNascimentoAdulto);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AtualizarRefreshToken_DeveDefinirTokenEExpiracao()
    {
        var usuario = Usuario.Criar("Teste", "teste@email.com", "hash", DataNascimentoAdulto);
        var token = "refresh_token_abc123";
        var expiry = DateTime.UtcNow.AddDays(7);

        usuario.AtualizarRefreshToken(token, expiry);

        usuario.RefreshToken.Should().Be(token);
        usuario.RefreshTokenExpiry.Should().BeCloseTo(expiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RefreshTokenValido_ComTokenCorretoENaoExpirado_DeveRetornarTrue()
    {
        var usuario = Usuario.Criar("Teste", "teste@email.com", "hash", DataNascimentoAdulto);
        var token = "meu_token_valido";
        usuario.AtualizarRefreshToken(token, DateTime.UtcNow.AddDays(7));

        usuario.RefreshTokenValido(token).Should().BeTrue();
    }

    [Fact]
    public void RefreshTokenValido_ComTokenExpirado_DeveRetornarFalse()
    {
        var usuario = Usuario.Criar("Teste", "teste@email.com", "hash", DataNascimentoAdulto);
        var token = "meu_token";
        usuario.AtualizarRefreshToken(token, DateTime.UtcNow.AddSeconds(-1));

        usuario.RefreshTokenValido(token).Should().BeFalse();
    }

    [Fact]
    public void RefreshTokenValido_ComTokenDiferente_DeveRetornarFalse()
    {
        var usuario = Usuario.Criar("Teste", "teste@email.com", "hash", DataNascimentoAdulto);
        usuario.AtualizarRefreshToken("token_correto", DateTime.UtcNow.AddDays(7));

        usuario.RefreshTokenValido("token_errado").Should().BeFalse();
    }

    [Fact]
    public void Desativar_DeveDefinirAtivoComoFalso()
    {
        var usuario = Usuario.Criar("Teste", "teste@email.com", "hash", DataNascimentoAdulto);
        usuario.Desativar();
        usuario.Ativo.Should().BeFalse();
    }
}
