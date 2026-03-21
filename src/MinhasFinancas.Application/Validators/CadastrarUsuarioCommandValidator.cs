using FluentValidation;
using MinhasFinancas.Application.Commands.Auth;

namespace MinhasFinancas.Application.Validators;

public class CadastrarUsuarioCommandValidator : AbstractValidator<CadastrarUsuarioCommand>
{
    private static readonly string[] SenhasComuns = { "Senha@123", "Password@1", "Admin@1234", "123456@Ab" };

    public CadastrarUsuarioCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(3).WithMessage("Nome deve ter pelo menos 3 caracteres.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(12).WithMessage("Senha deve ter pelo menos 12 caracteres.")
            .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter pelo menos um caractere especial.")
            .Must(s => !SenhasComuns.Contains(s)).WithMessage("Essa senha é muito comum. Escolha uma senha mais segura.");

        RuleFor(x => x.DataNascimento)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória.")
            .Must(d => {
                var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
                var idade = hoje.Year - d.Year;
                if (d > hoje.AddYears(-idade)) idade--;
                return idade >= 18;
            }).WithMessage("Usuário deve ter pelo menos 18 anos.");

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.")
            .When(x => x.Telefone != null);
    }
}
