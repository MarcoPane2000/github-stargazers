using FluentValidation;
using GitHubStargazers.WebApi.Dtos;

namespace GitHubStargazers.WebApi.Validators;

public class LoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("Username or Email is required.")
            .MinimumLength(3).WithMessage("Username or Email must be at least 3 characters long.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(3).WithMessage("Password must be at least 3 characters long.");
    }
}