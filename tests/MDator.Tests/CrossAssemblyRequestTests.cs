using Microsoft.Extensions.DependencyInjection;
using MDator.Tests.CrossAssembly;

namespace MDator.Tests;

public sealed class CrossAssemblyRequestTests
{
    [Fact]
    public async Task Send_CrossAssemblyRequest_DispatchesWithoutFallback()
    {
        await using var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send(new CrossGetItemQuery(42));

        Assert.Equal(42, result.Id);
        Assert.Equal("Item-42", result.Name);
    }

    [Fact]
    public async Task Send_CrossAssemblyVoidRequest_Completes()
    {
        var before = CrossDeleteHandler.DeleteCount;
        await using var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Send(new CrossDeleteCommand(1));

        Assert.Equal(before + 1, CrossDeleteHandler.DeleteCount);
    }

    [Fact]
    public async Task CreateStream_CrossAssemblyStream_YieldsExpectedItems()
    {
        await using var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var items = new List<CrossItem>();
        await foreach (var item in mediator.CreateStream(new CrossStreamItems(3)))
        {
            items.Add(item);
        }

        Assert.Equal(3, items.Count);
        Assert.Equal("Stream-0", items[0].Name);
        Assert.Equal("Stream-2", items[2].Name);
    }

    [Fact]
    public async Task Publish_CrossAssemblyNotification_InvokesHandler()
    {
        var before = CrossItemCreatedHandler.NotifyCount;
        await using var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        await mediator.Publish(new CrossItemCreated(1));

        Assert.Equal(before + 1, CrossItemCreatedHandler.NotifyCount);
    }

    [Fact]
    public async Task SendObject_CrossAssemblyRequest_DispatchesCorrectly()
    {
        await using var sp = TestServices.Build();
        var mediator = sp.GetRequiredService<IMediator>();

        var result = await mediator.Send((object)new CrossGetItemQuery(99));

        Assert.IsType<CrossItem>(result);
        Assert.Equal(99, ((CrossItem)result!).Id);
    }
}
