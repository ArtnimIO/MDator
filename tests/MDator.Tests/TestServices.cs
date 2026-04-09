using MDator;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

/// <summary>
/// Shared composition root for tests. <see cref="Log"/> is a singleton every test
/// can snapshot; open-generic behaviors declared via
/// <c>[assembly: MDator.OpenBehavior(...)]</c> depend on it.
/// </summary>
internal static class TestServices
{
    public static ServiceProvider Build(Action<MDatorConfiguration>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<Log>();
        services.AddMDator(configure);
        return services.BuildServiceProvider();
    }
}
