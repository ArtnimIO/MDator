using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

public record OrderPlaced(int OrderId) : INotification;

public sealed class EmailNotifier : INotificationHandler<OrderPlaced>
{
    private readonly Log _log;
    public EmailNotifier(Log log) => _log = log;
    public Task Handle(OrderPlaced n, CancellationToken ct)
    {
        _log.Entries.Add($"email:{n.OrderId}");
        return Task.CompletedTask;
    }
}

public sealed class AuditNotifier : INotificationHandler<OrderPlaced>
{
    private readonly Log _log;
    public AuditNotifier(Log log) => _log = log;
    public Task Handle(OrderPlaced n, CancellationToken ct)
    {
        _log.Entries.Add($"audit:{n.OrderId}");
        return Task.CompletedTask;
    }
}

public class NotificationTests
{
    [Fact]
    public async Task Publish_fanouts_to_every_handler()
    {
        var sp = TestServices.Build();
        var log = sp.GetRequiredService<Log>();
        var mediator = sp.GetRequiredService<IMediator>();
        await mediator.Publish(new OrderPlaced(7));

        Assert.Contains("email:7", log.Entries);
        Assert.Contains("audit:7", log.Entries);
    }

    [Fact]
    public async Task Publish_with_TaskWhenAll_still_hits_all_handlers()
    {
        var sp = TestServices.Build(cfg => cfg.NotificationPublisher = new TaskWhenAllPublisher());
        var log = sp.GetRequiredService<Log>();
        var mediator = sp.GetRequiredService<IMediator>();
        await mediator.Publish(new OrderPlaced(99));

        Assert.Contains("email:99", log.Entries);
        Assert.Contains("audit:99", log.Entries);
    }
}
