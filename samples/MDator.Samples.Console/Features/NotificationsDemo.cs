using MDator;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Samples.Console.Features;

// --- Notification ---

public record ProductCreatedNotification(Guid ProductId, string ProductName) : INotification;

// --- Handlers ---

public sealed class InventoryIndexer : INotificationHandler<ProductCreatedNotification>
{
    public Task Handle(ProductCreatedNotification notification, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkCyan;
        System.Console.WriteLine($"  [InventoryIndexer] Indexed product {notification.ProductName}");
        System.Console.ResetColor();
        return Task.CompletedTask;
    }
}

public sealed class AuditLogger : INotificationHandler<ProductCreatedNotification>
{
    public Task Handle(ProductCreatedNotification notification, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
        System.Console.WriteLine($"  [AuditLogger] Logged creation of {notification.ProductName}");
        System.Console.ResetColor();
        return Task.CompletedTask;
    }
}

public sealed class EmailSender : INotificationHandler<ProductCreatedNotification>
{
    public Task Handle(ProductCreatedNotification notification, CancellationToken ct)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkMagenta;
        System.Console.WriteLine($"  [EmailSender] Sent email for {notification.ProductName}");
        System.Console.ResetColor();
        return Task.CompletedTask;
    }
}

// --- Demo ---

public static class NotificationsDemo
{
    public static async Task RunAsync()
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine("--- 2. Notifications ---");
        System.Console.ResetColor();
        System.Console.WriteLine("Demonstrates Publish() with multiple handlers and different publisher strategies.\n");

        var notification = new ProductCreatedNotification(Guid.NewGuid(), "Gadget");

        // Sequential (default)
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[ForEachAwaitPublisher — Sequential]");
        System.Console.ResetColor();
        {
            var services = new ServiceCollection();
            services.AddMDator();
            await using var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<IMediator>();
            await mediator.Publish(notification);
        }

        System.Console.WriteLine();

        // Parallel
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine("[TaskWhenAllPublisher — Parallel]");
        System.Console.ResetColor();
        {
            var services = new ServiceCollection();
            services.AddMDator(cfg => cfg.NotificationPublisher = new TaskWhenAllPublisher());
            await using var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<IMediator>();
            await mediator.Publish(notification);
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Both strategies delivered to all 3 handlers. Sequential guarantees order;");
        System.Console.WriteLine("TaskWhenAll runs them concurrently.");
    }
}
