using FluentValidation;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Console.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Samples.Console.Features;

// --- Validator ---

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required.");
        RuleFor(x => x.Sku).NotEmpty().WithMessage("SKU is required.");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}

// --- Validation Behavior ---

public sealed class DemoValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var validatorList = validators.ToList();
        if (validatorList.Count == 0)
            return await next();

        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine($"  [Validation] Validating {typeof(TRequest).Name} with {validatorList.Count} validator(s)...");
        System.Console.ResetColor();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validatorList.Select(v => v.ValidateAsync(context, ct)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        System.Console.ForegroundColor = ConsoleColor.DarkGreen;
        System.Console.WriteLine("  [Validation] Passed!");
        System.Console.ResetColor();
        return await next();
    }
}

// --- Demo ---

public static class ValidationDemo
{
    public static async Task RunAsync()
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("--- 5. Validation ---");
        System.Console.ResetColor();
        System.Console.WriteLine("Demonstrates a FluentValidation pipeline behavior rejecting invalid input.\n");

        var services = new ServiceCollection();
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
        services.AddMDator(cfg => cfg.AddBehavior(
            typeof(IPipelineBehavior<,>),
            typeof(DemoValidationBehavior<,>)));
        await using var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<IMediator>();

        // Valid request
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[Valid Request]");
        System.Console.ResetColor();
        var product = await mediator.Send(new CreateProductCommand("Widget", "WDG-001", 9.99m));
        System.Console.WriteLine($"  Created: {product.Name}\n");

        // Invalid request — empty name, negative price
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[Invalid Request — empty name, negative price]");
        System.Console.ResetColor();
        try
        {
            await mediator.Send(new CreateProductCommand("", "WDG-002", -5.00m));
        }
        catch (ValidationException ex)
        {
            System.Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("  Validation failed:");
            foreach (var error in ex.Errors)
                System.Console.WriteLine($"    - {error.PropertyName}: {error.ErrorMessage}");
            System.Console.ResetColor();
        }
    }
}
