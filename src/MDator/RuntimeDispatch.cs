using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MDator;

/// <summary>
/// DI-based runtime dispatch for request types not advertised via
/// <c>[assembly: KnownRequest]</c> on any referenced assembly. Most cross-assembly
/// requests are now handled by the compile-time switch (the generator reads
/// <see cref="KnownRequestAttribute"/> from referenced assemblies). This fallback
/// only activates for truly dynamic or plugin-loaded request types. Called from
/// generated code only.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RuntimeDispatch
{
    // ── Cached MethodInfo for MakeGenericMethod ─────────────────────────

    private static readonly MethodInfo s_sendTyped =
        typeof(RuntimeDispatch).GetMethod(nameof(SendFallbackTyped), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo s_sendVoidTyped =
        typeof(RuntimeDispatch).GetMethod(nameof(SendVoidFallbackTyped), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo s_streamTyped =
        typeof(RuntimeDispatch).GetMethod(nameof(StreamFallbackTyped), BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo s_streamObjectTyped =
        typeof(RuntimeDispatch).GetMethod(nameof(StreamObjectFallbackTyped), BindingFlags.Static | BindingFlags.NonPublic)!;

    // ── Request with response ───────────────────────────────────────────

    /// <summary>
    /// Fallback for <c>Send&lt;TResponse&gt;(IRequest&lt;TResponse&gt;)</c> when
    /// the request type is not in the compile-time switch.
    /// </summary>
    public static Task<TResponse> SendFallback<TResponse>(
        IServiceProvider sp, MDatorConfiguration cfg,
        IRequest<TResponse> request, CancellationToken ct)
    {
        var method = s_sendTyped.MakeGenericMethod(request.GetType(), typeof(TResponse));
        return (Task<TResponse>)method.Invoke(null, new object[] { sp, cfg, request, ct })!;
    }

    private static async Task<TResponse> SendFallbackTyped<TRequest, TResponse>(
        IServiceProvider sp, MDatorConfiguration cfg,
        TRequest request, CancellationToken ct)
        where TRequest : IRequest<TResponse>
    {
        var handler = sp.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

        RequestHandlerDelegate<TResponse> next = async () =>
        {
            foreach (var p in sp.GetServices<IRequestPreProcessor<TRequest>>())
                await p.Process(request, ct).ConfigureAwait(false);

            var resp = await handler.Handle(request, ct).ConfigureAwait(false);

            foreach (var p in sp.GetServices<IRequestPostProcessor<TRequest, TResponse>>())
                await p.Process(request, resp, ct).ConfigureAwait(false);

            return resp;
        };

        next = ChainRequestBehaviors(sp, cfg, request, ct, next);
        return await next().ConfigureAwait(false);
    }

    // ── Void request ────────────────────────────────────────────────────

    /// <summary>
    /// Fallback for <c>Send&lt;TRequest&gt;(TRequest)</c> (void) when the request
    /// type is not in the compile-time switch.
    /// </summary>
    public static Task SendVoidFallback<TRequest>(
        IServiceProvider sp, MDatorConfiguration cfg,
        TRequest request, CancellationToken ct)
        where TRequest : IRequest
    {
        var method = s_sendVoidTyped.MakeGenericMethod(request!.GetType());
        return (Task)method.Invoke(null, new object[] { sp, cfg, request, ct })!;
    }

    private static async Task SendVoidFallbackTyped<TRequest>(
        IServiceProvider sp, MDatorConfiguration cfg,
        TRequest request, CancellationToken ct)
        where TRequest : IRequest
    {
        var handler = sp.GetRequiredService<IRequestHandler<TRequest>>();

        RequestHandlerDelegate<Unit> next = async () =>
        {
            foreach (var p in sp.GetServices<IRequestPreProcessor<TRequest>>())
                await p.Process(request, ct).ConfigureAwait(false);

            await handler.Handle(request, ct).ConfigureAwait(false);
            return Unit.Value;
        };

        next = ChainVoidBehaviors(sp, cfg, request, ct, next);
        await next().ConfigureAwait(false);
    }

    // ── Send(object) ────────────────────────────────────────────────────

    /// <summary>
    /// Fallback for <c>Send(object)</c> when the request type is not in the
    /// compile-time switch.
    /// </summary>
    public static async Task<object?> SendObjectFallback(
        IServiceProvider sp, MDatorConfiguration cfg,
        object request, CancellationToken ct)
    {
        var requestType = request.GetType();

        // Check for IRequest<TResponse>
        foreach (var iface in requestType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IRequest<>))
            {
                var responseType = iface.GetGenericArguments()[0];
                var method = s_sendTyped.MakeGenericMethod(requestType, responseType);
                var task = (Task)method.Invoke(null, new object[] { sp, cfg, request, ct })!;
                await task.ConfigureAwait(false);
                return task.GetType().GetProperty("Result")!.GetValue(task);
            }
        }

        // Check for IRequest (void)
        if (request is IRequest)
        {
            var method = s_sendVoidTyped.MakeGenericMethod(requestType);
            await ((Task)method.Invoke(null, new object[] { sp, cfg, request, ct })!).ConfigureAwait(false);
            return null;
        }

        throw new InvalidOperationException(
            $"MDator: no handler known for request type '{requestType.FullName}'. " +
            "The type does not implement IRequest or IRequest<TResponse>.");
    }

    // ── Stream with response ────────────────────────────────────────────

    /// <summary>
    /// Fallback for <c>CreateStream&lt;TResponse&gt;(IStreamRequest&lt;TResponse&gt;)</c>
    /// when the request type is not in the compile-time switch.
    /// </summary>
    public static IAsyncEnumerable<TResponse> StreamFallback<TResponse>(
        IServiceProvider sp, MDatorConfiguration cfg,
        IStreamRequest<TResponse> request, CancellationToken ct)
    {
        var method = s_streamTyped.MakeGenericMethod(request.GetType(), typeof(TResponse));
        return (IAsyncEnumerable<TResponse>)method.Invoke(null, new object[] { sp, cfg, request, ct })!;
    }

    private static IAsyncEnumerable<TResponse> StreamFallbackTyped<TRequest, TResponse>(
        IServiceProvider sp, MDatorConfiguration cfg,
        TRequest request, CancellationToken ct)
        where TRequest : IStreamRequest<TResponse>
    {
        var handler = sp.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();
        StreamHandlerDelegate<TResponse> next = () => handler.Handle(request, ct);

        next = ChainStreamBehaviors(sp, cfg, request, ct, next);
        return next();
    }

    // ── CreateStream(object) ────────────────────────────────────────────

    /// <summary>
    /// Fallback for <c>CreateStream(object)</c> when the request type is not
    /// in the compile-time switch.
    /// </summary>
    public static IAsyncEnumerable<object?> StreamObjectFallback(
        IServiceProvider sp, MDatorConfiguration cfg,
        object request, CancellationToken ct)
    {
        var requestType = request.GetType();
        foreach (var iface in requestType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IStreamRequest<>))
            {
                var responseType = iface.GetGenericArguments()[0];
                var method = s_streamObjectTyped.MakeGenericMethod(requestType, responseType);
                return (IAsyncEnumerable<object?>)method.Invoke(null, new object[] { sp, cfg, request, ct })!;
            }
        }

        throw new InvalidOperationException(
            $"MDator: no stream handler known for request type '{requestType.FullName}'. " +
            "The type does not implement IStreamRequest<TResponse>.");
    }

    private static async IAsyncEnumerable<object?> StreamObjectFallbackTyped<TRequest, TResponse>(
        IServiceProvider sp, MDatorConfiguration cfg,
        TRequest request, [EnumeratorCancellation] CancellationToken ct)
        where TRequest : IStreamRequest<TResponse>
    {
        await foreach (var item in StreamFallbackTyped<TRequest, TResponse>(sp, cfg, request, ct)
            .WithCancellation(ct).ConfigureAwait(false))
        {
            yield return item;
        }
    }

    // ── Behavior chaining helpers ───────────────────────────────────────

    private static RequestHandlerDelegate<TResponse> ChainRequestBehaviors<TRequest, TResponse>(
        IServiceProvider sp, MDatorConfiguration cfg,
        TRequest request, CancellationToken ct,
        RequestHandlerDelegate<TResponse> next)
    {
        // Open behaviors registered via [assembly: OpenBehavior(...)].
        // Reversed so that declared-earlier (lower Order) = outermost, matching
        // the compile-time fused pipeline ordering.
        foreach (var (openType, _) in cfg.OpenBehaviorTypes.OrderBy(x => x.Order).Reverse())
        {
            try
            {
                var closedType = openType.MakeGenericType(typeof(TRequest), typeof(TResponse));
                if (sp.GetService(closedType) is IPipelineBehavior<TRequest, TResponse> b)
                {
                    var prev = next;
                    next = () => b.Handle(request, prev, ct);
                }
            }
            catch (ArgumentException)
            {
                // Open type may not accept these type arguments (e.g. additional
                // constraints on the open generic). Skip gracefully.
            }
        }

        // Runtime-added behaviors (skipped under FuseOnly).
        if (!cfg.FuseOnly)
        {
            foreach (var rb in sp.GetServices<IPipelineBehavior<TRequest, TResponse>>())
            {
                var prev = next;
                next = () => rb.Handle(request, prev, ct);
            }
        }

        return next;
    }

    private static RequestHandlerDelegate<Unit> ChainVoidBehaviors<TRequest>(
        IServiceProvider sp, MDatorConfiguration cfg,
        TRequest request, CancellationToken ct,
        RequestHandlerDelegate<Unit> next)
    {
        foreach (var (openType, _) in cfg.OpenBehaviorTypes.OrderBy(x => x.Order).Reverse())
        {
            try
            {
                var closedType = openType.MakeGenericType(typeof(TRequest), typeof(Unit));
                if (sp.GetService(closedType) is IPipelineBehavior<TRequest, Unit> b)
                {
                    var prev = next;
                    next = () => b.Handle(request, prev, ct);
                }
            }
            catch (ArgumentException)
            {
                // Skip if type arguments don't match open generic constraints.
            }
        }

        if (!cfg.FuseOnly)
        {
            foreach (var rb in sp.GetServices<IPipelineBehavior<TRequest, Unit>>())
            {
                var prev = next;
                next = () => rb.Handle(request, prev, ct);
            }
        }

        return next;
    }

    private static StreamHandlerDelegate<TResponse> ChainStreamBehaviors<TRequest, TResponse>(
        IServiceProvider sp, MDatorConfiguration cfg,
        TRequest request, CancellationToken ct,
        StreamHandlerDelegate<TResponse> next)
        where TRequest : IStreamRequest<TResponse>
    {
        foreach (var (openType, _) in cfg.OpenBehaviorTypes.OrderBy(x => x.Order).Reverse())
        {
            try
            {
                var closedType = openType.MakeGenericType(typeof(TRequest), typeof(TResponse));
                if (sp.GetService(closedType) is IStreamPipelineBehavior<TRequest, TResponse> b)
                {
                    var prev = next;
                    next = () => b.Handle(request, prev, ct);
                }
            }
            catch (ArgumentException)
            {
                // Skip if type arguments don't match open generic constraints.
            }
        }

        if (!cfg.FuseOnly)
        {
            foreach (var rb in sp.GetServices<IStreamPipelineBehavior<TRequest, TResponse>>())
            {
                var prev = next;
                next = () => rb.Handle(request, prev, ct);
            }
        }

        return next;
    }
}
