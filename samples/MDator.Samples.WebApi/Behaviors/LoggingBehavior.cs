namespace MDator.Samples.WebApi.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        logger.LogInformation("Handling {RequestType}: {@Request}", typeof(TRequest).Name, request);
        var response = await next();
        logger.LogInformation("Handled {RequestType} -> {ResponseType}", typeof(TRequest).Name, typeof(TResponse).Name);
        return response;
    }
}
