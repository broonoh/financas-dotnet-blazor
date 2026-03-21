using FluentValidation;
using MinhasFinancas.Application.Commands.Dividas;

namespace MinhasFinancas.Application.Validators;

public class CriarDividaCommandValidator : AbstractValidator<CriarDividaCommand>
{
    public CriarDividaCommandValidator()
    {
        RuleFor(x => x.NomeDevedor)
            .NotEmpty().WithMessage("Nome do devedor é obrigatório.")
            .MinimumLength(2).WithMessage("Nome do devedor deve ter pelo menos 2 caracteres.")
            .MaximumLength(100).WithMessage("Nome do devedor deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MinimumLength(3).WithMessage("Descrição deve ter pelo menos 3 caracteres.")
            .MaximumLength(200).WithMessage("Descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.ValorTotal)
            .GreaterThan(0).WithMessage("Valor total deve ser maior que zero.");

        RuleFor(x => x.QuantidadeParcelas)
            .InclusiveBetween(1, 120).WithMessage("Quantidade de parcelas deve ser entre 1 e 120.");
    }
}
