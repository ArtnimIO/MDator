using MDator;
using MDator.Samples.WebApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace MDator.Samples.WebApi.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(
    AppDbContext db,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        // Only wrap commands (types ending in "Command") in a transaction
        if (!typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal))
            return await next();

        // InMemory provider doesn't support transactions — skip gracefully
        if (db.Database.IsInMemory())
        {
            logger.LogDebug("Skipping transaction for {RequestType} (InMemory provider)", typeof(TRequest).Name);
            return await next();
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        logger.LogDebug("Started transaction for {RequestType}", typeof(TRequest).Name);

        try
        {
            var response = await next();
            await transaction.CommitAsync(ct);
            logger.LogDebug("Committed transaction for {RequestType}", typeof(TRequest).Name);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            logger.LogWarning("Rolled back transaction for {RequestType}", typeof(TRequest).Name);
            throw;
        }
    }
}
