using FluentAssertions;
using MinhasFinancas.Domain.Entities;
using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Tests.Unit;

public class DespesaFixaTests
{
    private static readonly Guid UsuarioId = Guid.NewGuid();
    private static readonly DateOnly DataFutura = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

    [Fact]
    public void Criar_ComDadosValidos_DeveGerarParcelas()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Financiamento Carro", 1200.00m, 12, DataFutura,
            CategoriaDespesa.Transporte, FormaPagamentoDespesaFixa.CartaoCredito);

        despesa.Parcelas.Should().HaveCount(12);
        despesa.ValorTotal.Should().Be(1200.00m);
    }

    [Fact]
    public void Criar_SomaDasParcelasDeveIgualValorTotal()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Notebook", 999.99m, 12, DataFutura,
            CategoriaDespesa.Educacao, FormaPagamentoDespesaFixa.CartaoCredito);

        var soma = despesa.Parcelas.Sum(p => p.Valor);
        soma.Should().Be(999.99m);
    }

    [Fact]
    public void Criar_UltimaParcela_DeveAbsorverResto()
    {
        // 100 / 3 = 33.33 (centavos: 10000 / 3 = 3333 resto 1)
        // Parcelas 1 e 2: R$ 33,33; Parcela 3: R$ 33,34
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Curso Online", 100.00m, 3, DataFutura,
            CategoriaDespesa.Educacao, FormaPagamentoDespesaFixa.BoletoParcelado);

        var parcelas = despesa.Parcelas.ToList();
        parcelas[0].Valor.Should().Be(33.33m);
        parcelas[1].Valor.Should().Be(33.33m);
        parcelas[2].Valor.Should().Be(33.34m);

        parcelas.Sum(p => p.Valor).Should().Be(100.00m);
    }

    [Fact]
    public void Criar_ParcelasDevemTerDatasSequenciais()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Academia", 360m, 6, DataFutura,
            CategoriaDespesa.Saude, FormaPagamentoDespesaFixa.CartaoCredito);

        for (int i = 0; i < despesa.Parcelas.Count; i++)
        {
            despesa.Parcelas[i].DataVencimento.Should().Be(DataFutura.AddMonths(i));
            despesa.Parcelas[i].Numero.Should().Be(i + 1);
        }
    }

    [Fact]
    public void Criar_TodasParcelasDevemEstarNaoPagas()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Plano de Saúde", 240m, 12, DataFutura,
            CategoriaDespesa.Saude, FormaPagamentoDespesaFixa.CartaoCredito);

        despesa.Parcelas.Should().AllSatisfy(p => p.Paga.Should().BeFalse());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(49)]
    public void Criar_ComQuantidadeParcelasInvalida_DeveLancarExcecao(int quantidade)
    {
        var act = () => DespesaFixa.Criar(
            UsuarioId, "Teste", 100m, quantidade, DataFutura,
            CategoriaDespesa.Outros, FormaPagamentoDespesaFixa.CartaoCredito);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*parcelas*");
    }

    [Fact]
    public void Criar_ComValorZero_DeveLancarExcecao()
    {
        var act = () => DespesaFixa.Criar(
            UsuarioId, "Teste", 0m, 3, DataFutura,
            CategoriaDespesa.Outros, FormaPagamentoDespesaFixa.CartaoCredito);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Valor total*");
    }

    [Fact]
    public void Criar_ComDataAnteriorAoMesAtual_DeveLancarExcecao()
    {
        var dataPasada = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var act = () => DespesaFixa.Criar(
            UsuarioId, "Teste", 100m, 3, dataPasada,
            CategoriaDespesa.Outros, FormaPagamentoDespesaFixa.CartaoCredito);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*anterior*");
    }

    [Fact]
    public void Criar_ComDescricaoCurta_DeveLancarExcecao()
    {
        var act = () => DespesaFixa.Criar(
            UsuarioId, "AB", 100m, 3, DataFutura,
            CategoriaDespesa.Outros, FormaPagamentoDespesaFixa.CartaoCredito);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_ValorExato_NaoDeveHaverDiferencaDeArredondamento()
    {
        // 1000 / 4 = 250.00 exato, sem resto
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Empréstimo", 1000m, 4, DataFutura,
            CategoriaDespesa.Outros, FormaPagamentoDespesaFixa.PixParcelado);

        despesa.Parcelas.Should().AllSatisfy(p => p.Valor.Should().Be(250m));
        despesa.Parcelas.Sum(p => p.Valor).Should().Be(1000m);
    }
}
