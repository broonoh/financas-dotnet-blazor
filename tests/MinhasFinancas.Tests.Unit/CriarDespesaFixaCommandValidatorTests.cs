using FluentAssertions;
using MinhasFinancas.Application.Commands.Despesas;
using MinhasFinancas.Application.Validators;
using MinhasFinancas.Domain.Enums;

namespace MinhasFinancas.Tests.Unit;

public class CriarDespesaFixaCommandValidatorTests
{
    private readonly CriarDespesaFixaCommandValidator _validator = new();
    private static readonly DateOnly DataFutura = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

    private static readonly DateOnly DataCompra = DateOnly.FromDateTime(DateTime.UtcNow);

    private CriarDespesaFixaCommand ComandoValido() => new(
        Guid.NewGuid(),
        "Financiamento Veículo",
        12000m,
        36,
        DataCompra,
        DataFutura,
        "Transporte",
        FormaPagamentoDespesaFixa.CartaoCredito);

    [Fact]
    public void Validar_ComDadosValidos_DevePassar()
    {
        var result = _validator.Validate(ComandoValido());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    public void Validar_ComDescricaoInvalida_DeveFalhar(string descricao)
    {
        var command = ComandoValido() with { Descricao = descricao };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Descricao");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validar_ComValorInvalido_DeveFalhar(decimal valor)
    {
        var command = ComandoValido() with { ValorTotal = valor };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ValorTotal");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(49)]
    public void Validar_ComQuantidadeParcelasInvalida_DeveFalhar(int qtd)
    {
        var command = ComandoValido() with { QuantidadeParcelas = qtd };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "QuantidadeParcelas");
    }

    [Fact]
    public void Validar_ComDataAnteriorAoMesAtual_DeveFalhar()
    {
        var dataPassada = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2));
        var command = ComandoValido() with { DataPrimeiraParcela = dataPassada };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DataPrimeiraParcela");
    }

    [Fact]
    public void Validar_ComMesAtual_DevePassar()
    {
        var mesAtual = DateOnly.FromDateTime(DateTime.UtcNow);
        var command = ComandoValido() with { DataPrimeiraParcela = mesAtual };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
