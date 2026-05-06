using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

// Regression for: AddMDator must tolerate registration callbacks that mutate
// MDatorGeneratedHook.Registrations during iteration. In production this is
// hit when invoking a callback triggers the JIT to load a not-yet-loaded
// handler-bearing assembly, whose module initializer appends to the hook list.
// A `foreach` over List<T> throws "Collection was modified" in that case,
// taking down anything calling AddMDator at composition root.
public class RegistrationHookRaceTests
{
    [Fact]
    public void AddMDator_tolerates_callback_appending_more_registrations()
    {
        var ranEarlyArrival = false;
        var ranLateArrival = false;

        Action<IServiceCollection, MDatorConfiguration> lateArrival =
            (_, _) => ranLateArrival = true;

        Action<IServiceCollection, MDatorConfiguration> earlyArrival =
            (_, _) =>
            {
                ranEarlyArrival = true;
                // Simulate a module initializer firing mid-iteration.
                MDatorGeneratedHook.Registrations.Add(lateArrival);
            };

        var startCount = MDatorGeneratedHook.Registrations.Count;
        MDatorGeneratedHook.Registrations.Add(earlyArrival);

        try
        {
            var services = new ServiceCollection();
            services.AddMDator();

            Assert.True(ranEarlyArrival);
            Assert.True(ranLateArrival);
        }
        finally
        {
            // Pop both callbacks so we don't pollute later tests. Late arrival
            // was appended at the end during AddMDator; early arrival is at
            // the position we put it. Trim back to startCount.
            while (MDatorGeneratedHook.Registrations.Count > startCount)
            {
                MDatorGeneratedHook.Registrations.RemoveAt(
                    MDatorGeneratedHook.Registrations.Count - 1);
            }
        }
    }
}
