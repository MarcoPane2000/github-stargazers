using FluentValidation;

namespace GitHubStargazers.WebApi.Validators;

public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator = validator;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argToValidate = context.Arguments.FirstOrDefault(x => x is T) as T;

        if (argToValidate is not null)
        {
            var validationResult = await _validator.ValidateAsync(argToValidate);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
        }

        return await next(context);
    }
}