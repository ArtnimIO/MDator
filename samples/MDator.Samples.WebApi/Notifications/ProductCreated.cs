namespace MDator.Samples.WebApi.Notifications;

public record ProductCreatedNotification(Guid ProductId, string ProductName) : INotification;

public sealed class ProductCreatedIndexHandler(ILogger<ProductCreatedIndexHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    public Task Handle(ProductCreatedNotification notification, CancellationToken ct)
    {
        logger.LogInformation("Indexing new product: {ProductName} ({ProductId})",
            notification.ProductName, notification.ProductId);
        return Task.CompletedTask;
    }
}

public sealed class ProductCreatedAuditHandler(ILogger<ProductCreatedAuditHandler> logger)
    : INotificationHandler<ProductCreatedNotification>
{
    public Task Handle(ProductCreatedNotification notification, CancellationToken ct)
    {
        logger.LogInformation("Audit: Product created -- {ProductName} ({ProductId})",
            notification.ProductName, notification.ProductId);
        return Task.CompletedTask;
    }
}
