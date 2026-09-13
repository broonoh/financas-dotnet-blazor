using FluentAssertions;
using MinhasFinancas.Domain.Entities;

namespace MinhasFinancas.Tests.Unit;

public class DespesaFixaTests
{
    private static readonly Guid UsuarioId = Guid.NewGuid();
    private static readonly DateOnly DataCompra = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly DateOnly DataFutura = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

    [Fact]
    public void Criar_ComDadosValidos_DeveGerarParcelas()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Financiamento Carro", 100.00m, 12, DataCompra, DataFutura,
            "Transporte", "Cartão de Crédito");

        despesa.Parcelas.Should().HaveCount(12);
        despesa.ValorTotal.Should().Be(1200.00m);
    }

    [Fact]
    public void Criar_ValorTotalDeveSerValorDaParcelaVezesQuantidade()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Notebook", 83.33m, 12, DataCompra, DataFutura,
            "Educação", "Cartão de Crédito");

        despesa.ValorTotal.Should().Be(83.33m * 12);
        despesa.Parcelas.Sum(p => p.Valor).Should().Be(despesa.ValorTotal);
    }

    [Fact]
    public void Criar_TodasParcelasDevemTerOMesmoValor()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Curso Online", 33.33m, 3, DataCompra, DataFutura,
            "Educação", "Boleto Parcelado");

        despesa.Parcelas.Should().AllSatisfy(p => p.Valor.Should().Be(33.33m));
    }

    [Fact]
    public void Criar_ParcelasDevemTerDatasSequenciais()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Academia", 30m, 6, DataCompra, DataFutura,
            "Saúde", "Cartão de Crédito");

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
            UsuarioId, "Plano de Saúde", 20m, 12, DataCompra, DataFutura,
            "Saúde", "Cartão de Crédito");

        despesa.Parcelas.Should().AllSatisfy(p => p.Paga.Should().BeFalse());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(49)]
    public void Criar_ComQuantidadeParcelasInvalida_DeveLancarExcecao(int quantidade)
    {
        var act = () => DespesaFixa.Criar(
            UsuarioId, "Teste", 100m, quantidade, DataCompra, DataFutura,
            "Outros", "Cartão de Crédito");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*parcelas*");
    }

    [Fact]
    public void Criar_ComValorZero_DeveLancarExcecao()
    {
        var act = () => DespesaFixa.Criar(
            UsuarioId, "Teste", 0m, 3, DataCompra, DataFutura,
            "Outros", "Cartão de Crédito");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Valor da parcela*");
    }

    [Fact]
    public void Criar_ComDataAnteriorAoMesAtual_DeveLancarExcecao()
    {
        var dataPasada = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var act = () => DespesaFixa.Criar(
            UsuarioId, "Teste", 100m, 3, DataCompra, dataPasada,
            "Outros", "Cartão de Crédito");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*anterior*");
    }

    [Fact]
    public void Criar_ComDescricaoCurta_DeveLancarExcecao()
    {
        var act = () => DespesaFixa.Criar(
            UsuarioId, "AB", 100m, 3, DataCompra, DataFutura,
            "Outros", "Cartão de Crédito");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_ValorExato_NaoDeveHaverDiferencaDeArredondamento()
    {
        var despesa = DespesaFixa.Criar(
            UsuarioId, "Empréstimo", 250m, 4, DataCompra, DataFutura,
            "Outros", "Pix Parcelado");

        despesa.Parcelas.Should().AllSatisfy(p => p.Valor.Should().Be(250m));
        despesa.ValorTotal.Should().Be(1000m);
        despesa.Parcelas.Sum(p => p.Valor).Should().Be(1000m);
    }

    [Fact]
    public void Criar_DataCompraDiferenteDoPrimeiroVencimento_DeveUsarVencimentoNasParcelas()
    {
        var dataCompra = DateOnly.FromDateTime(DateTime.UtcNow);
        var vencimento = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)).AddDays(9); // dia 10 do mês seguinte

        var despesa = DespesaFixa.Criar(
            UsuarioId, "Cartão Compra", 100m, 3, dataCompra, vencimento,
            "Outros", "Cartão de Crédito");

        despesa.DataCompra.Should().Be(dataCompra);
        despesa.DataPrimeiraParcela.Should().Be(vencimento);
        despesa.Parcelas[0].DataVencimento.Should().Be(vencimento);
        despesa.Parcelas[1].DataVencimento.Should().Be(vencimento.AddMonths(1));
        despesa.Parcelas[2].DataVencimento.Should().Be(vencimento.AddMonths(2));
    }
}
