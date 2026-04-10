namespace MDator.Samples.WebApi.Notifications;

public record LowStockDetectedNotification(Guid ProductId, string ProductName, int CurrentQuantity) : INotification;

public sealed class LowStockAlertHandler(ILogger<LowStockAlertHandler> logger)
    : INotificationHandler<LowStockDetectedNotification>
{
    public Task Handle(LowStockDetectedNotification notification, CancellationToken ct)
    {
        logger.LogWarning("LOW STOCK ALERT: {ProductName} has only {CurrentQuantity} units remaining!",
            notification.ProductName, notification.CurrentQuantity);
        return Task.CompletedTask;
    }
}
