using MDator;
using Microsoft.Extensions.DependencyInjection;

namespace MDator.Tests;

public record CountStream(int To) : IStreamRequest<int>;

public sealed class CountStreamHandler : IStreamRequestHandler<CountStream, int>
{
    public async IAsyncEnumerable<int> Handle(CountStream request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        for (var i = 1; i <= request.To; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }
}

public class StreamTests
{
    [Fact]
    public async Task CreateStream_yields_all_items()
    {
        var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var items = new List<int>();
        await foreach (var i in mediator.CreateStream(new CountStream(3)))
            items.Add(i);

        Assert.Equal(new[] { 1, 2, 3 }, items);
    }
}
