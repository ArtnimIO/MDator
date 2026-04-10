using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MDator.SourceGenerator;

/// <summary>
/// A lightweight immutable array with value-based equality. Used throughout the
/// incremental pipeline instead of <see cref="System.Collections.Immutable.ImmutableArray{T}"/>,
/// which uses reference equality and therefore breaks IDE caching in incremental
/// generators.
/// </summary>
internal readonly struct EquatableArray<T>(T[] items) : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    public static readonly EquatableArray<T> Empty = new([]);

    private readonly T[] _items = items;

    public int Count => _items?.Length ?? 0;

    public T this[int index] => _items[index];

    public bool Equals(EquatableArray<T> other)
    {
        var a = _items ?? [];
        var b = other._items;
        if (a.Length != b.Length) return false;
        return !a.Where((t, i) => !EqualityComparer<T>.Default.Equals(t, b[i])).Any();
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> a && Equals(a);

    public override int GetHashCode()
    {
        if (_items is null) return 0;
        unchecked
        {
            return _items.Aggregate(17, (current, item) => current * 31 + (item?.GetHashCode() ?? 0));
        }
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_items ?? [])).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static EquatableArray<T> From(IEnumerable<T> source) => new(source.ToArray());

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}
