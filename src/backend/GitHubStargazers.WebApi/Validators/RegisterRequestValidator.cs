using FluentValidation;
using GitHubStargazers.WebApi.Dtos;

namespace GitHubStargazers.WebApi.Validators;

public class RegisterRequestValidator : AbstractValidator<UserRegistrationRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(3).WithMessage("Password must be at least 3 characters long.");

        RuleFor(x => x.FavoriteColor)
            .IsInEnum().WithMessage("The selected favorite color is invalid.");
    }
}