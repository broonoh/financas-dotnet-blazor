using FluentValidation;
using MinhasFinancas.Application.Commands.Receitas;

namespace MinhasFinancas.Application.Validators;

public class CriarReceitaCommandValidator : AbstractValidator<CriarReceitaCommand>
{
    public CriarReceitaCommandValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MinimumLength(3).WithMessage("Descrição deve ter pelo menos 3 caracteres.")
            .MaximumLength(100).WithMessage("Descrição deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
    }
}
