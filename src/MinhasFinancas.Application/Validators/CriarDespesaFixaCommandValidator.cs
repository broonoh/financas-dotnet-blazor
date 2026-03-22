using FluentValidation;
using MinhasFinancas.Application.Commands.Despesas;

namespace MinhasFinancas.Application.Validators;

public class CriarDespesaFixaCommandValidator : AbstractValidator<CriarDespesaFixaCommand>
{
    public CriarDespesaFixaCommandValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MinimumLength(3).WithMessage("Descrição deve ter pelo menos 3 caracteres.")
            .MaximumLength(100).WithMessage("Descrição deve ter no máximo 100 caracteres.");

        RuleFor(x => x.ValorTotal)
            .GreaterThan(0).WithMessage("Valor total deve ser maior que zero.");

        RuleFor(x => x.QuantidadeParcelas)
            .InclusiveBetween(2, 48).WithMessage("Quantidade de parcelas deve ser entre 2 e 48.");

        RuleFor(x => x.DataCompra)
            .NotEmpty().WithMessage("Data da compra é obrigatória.");

        RuleFor(x => x.DataPrimeiraParcela)
            .Must(d => {
                var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
                var inicioMesAtual = new DateOnly(hoje.Year, hoje.Month, 1);
                var inicioPrimeiraParcela = new DateOnly(d.Year, d.Month, 1);
                return inicioPrimeiraParcela >= inicioMesAtual;
            }).WithMessage("Data da primeira parcela não pode ser anterior ao mês atual.");
    }
}
