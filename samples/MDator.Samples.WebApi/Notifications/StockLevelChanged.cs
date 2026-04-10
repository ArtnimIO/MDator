using MDator;
using MDator.Samples.Domain.Interfaces;
using MDator.Samples.Domain.Models;

namespace MDator.Samples.WebApi.Notifications;

public record StockLevelChangedNotification(Guid ProductId, string ProductName, int NewQuantity, int LowStockThreshold = 5)
    : INotification;

public sealed class StockLevelChangedHandler(IPublisher publisher, IStockAlertRepository alertRepo, ILogger<StockLevelChangedHandler> logger)
    : INotificationHandler<StockLevelChangedNotification>
{
    public async Task Handle(StockLevelChangedNotification notification, CancellationToken ct)
    {
        logger.LogInformation("Stock level changed: {ProductName} -> {NewQuantity}",
            notification.ProductName, notification.NewQuantity);

        if (notification.NewQuantity == 0)
        {
            await alertRepo.AddAsync(new StockAlert
            {
                ProductId = notification.ProductId,
                AlertType = AlertType.OutOfStock,
                Threshold = notification.LowStockThreshold,
                CurrentQuantity = 0
            }, ct);
            await publisher.Publish(new LowStockDetectedNotification(notification.ProductId, notification.ProductName, 0), ct);
        }
        else if (notification.NewQuantity <= notification.LowStockThreshold)
        {
            await alertRepo.AddAsync(new StockAlert
            {
                ProductId = notification.ProductId,
                AlertType = AlertType.LowStock,
                Threshold = notification.LowStockThreshold,
                CurrentQuantity = notification.NewQuantity
            }, ct);
            await publisher.Publish(new LowStockDetectedNotification(notification.ProductId, notification.ProductName, notification.NewQuantity), ct);
        }
    }
}
