using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

// Regression for: calling AddMDator more than once on the same service
// collection replayed every generated registration callback again, so each
// INotificationHandler was registered once per call and fired that many times
// per publish. Hit in production by composition roots that call AddMDator once
// per feature module (background jobs executed 26x, AltensoSCM #917).
public class DuplicateRegistrationTests
{
  [Fact]
  public async Task AddMDator_called_twice_invokes_each_notification_handler_once()
  {
    var services = new ServiceCollection();
    services.AddSingleton<Log>();
    services.AddMDator();
    services.AddMDator();
    await using var sp = services.BuildServiceProvider();

    var log = sp.GetRequiredService<Log>();
    var mediator = sp.GetRequiredService<IMediator>();
    await mediator.Publish(new OrderPlaced(7));

    Assert.Equal(1, log.Entries.Count(e => e == "email:7"));
    Assert.Equal(1, log.Entries.Count(e => e == "audit:7"));
  }

  [Fact]
  public void AddMDator_called_twice_registers_each_handler_descriptor_once()
  {
    var services = new ServiceCollection();
    services.AddSingleton<Log>();
    services.AddMDator();
    var baseline = services.Count(d => d.ServiceType == typeof(INotificationHandler<OrderPlaced>));

    services.AddMDator();
    var after = services.Count(d => d.ServiceType == typeof(INotificationHandler<OrderPlaced>));

    Assert.Equal(baseline, after);
  }

  [Fact]
  public void AddMDator_on_a_fresh_collection_still_registers_everything()
  {
    // Idempotency must be per service collection, not process-global.
    var first = new ServiceCollection();
    first.AddSingleton<Log>();
    first.AddMDator();

    var second = new ServiceCollection();
    second.AddSingleton<Log>();
    second.AddMDator();

    var firstCount = first.Count(d => d.ServiceType == typeof(INotificationHandler<OrderPlaced>));
    var secondCount = second.Count(d => d.ServiceType == typeof(INotificationHandler<OrderPlaced>));

    Assert.NotEqual(0, secondCount);
    Assert.Equal(firstCount, secondCount);
  }
}
