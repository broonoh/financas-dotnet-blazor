using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Tests.Unit;

public class ParcelaTests
{
    private static readonly Guid DespesaId = Guid.NewGuid();

    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarParcelaNaoPaga()
    {
        var parcela = Parcela.Criar(DespesaId, 1, 100m, new DateOnly(2025, 1, 1));

        parcela.Id.Should().NotBeEmpty();
        parcela.DespesaId.Should().Be(DespesaId);
        parcela.Numero.Should().Be(1);
        parcela.Valor.Should().Be(100m);
        parcela.Paga.Should().BeFalse();
        parcela.DataPagamento.Should().BeNull();
    }

    [Fact]
    public void MarcarPaga_SemData_DeveUsarDataAtual()
    {
        var parcela = Parcela.Criar(DespesaId, 1, 100m, new DateOnly(2025, 1, 1));
        var antes = DateOnly.FromDateTime(DateTime.UtcNow);

        parcela.MarcarPaga();

        parcela.Paga.Should().BeTrue();
        parcela.DataPagamento.Should().NotBeNull();
        (parcela.DataPagamento!.Value >= antes).Should().BeTrue();
    }

    [Fact]
    public void MarcarPaga_ComData_DeveUsarDataInformada()
    {
        var parcela = Parcela.Criar(DespesaId, 1, 100m, new DateOnly(2025, 1, 1));
        var dataPagamento = new DateOnly(2025, 1, 15);

        parcela.MarcarPaga(dataPagamento);

        parcela.Paga.Should().BeTrue();
        parcela.DataPagamento.Should().Be(dataPagamento);
    }

    [Fact]
    public void DesmarcarPaga_DeveLimparDataPagamento()
    {
        var parcela = Parcela.Criar(DespesaId, 1, 100m, new DateOnly(2025, 1, 1));
        parcela.MarcarPaga(new DateOnly(2025, 1, 10));

        parcela.DesmarcarPaga();

        parcela.Paga.Should().BeFalse();
        parcela.DataPagamento.Should().BeNull();
    }

    [Fact]
    public void AtualizarValor_ComValorPositivo_DeveAtualizar()
    {
        var parcela = Parcela.Criar(DespesaId, 1, 100m, new DateOnly(2025, 1, 1));
        parcela.AtualizarValor(150m);
        parcela.Valor.Should().Be(150m);
    }

    [Fact]
    public void AtualizarValor_ComZero_DeveLancarExcecao()
    {
        var parcela = Parcela.Criar(DespesaId, 1, 100m, new DateOnly(2025, 1, 1));
        var act = () => parcela.AtualizarValor(0m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_ComValorNegativo_DeveLancarExcecao()
    {
        var act = () => Parcela.Criar(DespesaId, 1, -10m, new DateOnly(2025, 1, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_ComNumeroZero_DeveLancarExcecao()
    {
        var act = () => Parcela.Criar(DespesaId, 0, 100m, new DateOnly(2025, 1, 1));
        act.Should().Throw<ArgumentException>();
    }
}
