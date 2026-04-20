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
    /// <summary>
    /// Represents the singleton value of the <see cref="Unit"/> type, signifying the absence of a meaningful value.
    /// Commonly used for methods or operations that do not return a result but still fit into generic patterns.
    /// </summary>
    public static readonly Unit Value = default;

    /// <summary>
    /// A cached completed <see cref="Task{Unit}"/> so handlers can return
    /// <c>Unit.Task</c> without allocating.
    /// </summary>
    public static readonly Task<Unit> Task = System.Threading.Tasks.Task.FromResult(Value);

    /// <inheritdoc />
    public int CompareTo(Unit other) => 0;

    int IComparable.CompareTo(object? obj) => 0;

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public override string ToString() => "()";

    /// <summary>
    /// Determines whether two <see cref="Unit"/> instances are considered equal.
    /// </summary>
    /// <param name="first">The first <see cref="Unit"/> instance to compare.</param>
    /// <param name="second">The second <see cref="Unit"/> instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Unit first, Unit second) => true;

    /// <summary>
    /// Determines whether two <see cref="Unit"/> instances are not equal.
    /// </summary>
    /// <param name="first">The first <see cref="Unit"/> instance to compare.</param>
    /// <param name="second">The second <see cref="Unit"/> instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Unit first, Unit second) => false;
}
