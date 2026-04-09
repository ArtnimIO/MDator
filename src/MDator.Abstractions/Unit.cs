using System;
using System.Threading.Tasks;

namespace MDator;

/// <summary>
/// Represents a "no-value" return. Used by request handlers that execute an action
/// but produce no response, so they can still slot into the generic
/// <see cref="IRequest{TResponse}"/> / <see cref="IRequestHandler{TRequest, TResponse}"/>
/// machinery uniformly.
/// </summary>
public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
{
    public static readonly Unit Value = default;

    /// <summary>
    /// A cached completed <see cref="Task{Unit}"/> so handlers can return
    /// <c>Unit.Task</c> without allocating.
    /// </summary>
    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

    public int CompareTo(Unit other) => 0;

    int IComparable.CompareTo(object? obj) => 0;

    public bool Equals(Unit other) => true;

    public override bool Equals(object? obj) => obj is Unit;

    public override int GetHashCode() => 0;

    public override string ToString() => "()";

    public static bool operator ==(Unit first, Unit second) => true;

    public static bool operator !=(Unit first, Unit second) => false;
}
