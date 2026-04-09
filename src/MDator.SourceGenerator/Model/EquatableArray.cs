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
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    public static readonly EquatableArray<T> Empty = new(Array.Empty<T>());

    private readonly T[] _items;

    public EquatableArray(T[] items)
    {
        _items = items;
    }

    public int Count => _items?.Length ?? 0;

    public T this[int index] => _items![index];

    public bool Equals(EquatableArray<T> other)
    {
        var a = _items ?? Array.Empty<T>();
        var b = other._items ?? Array.Empty<T>();
        if (a.Length != b.Length) return false;
        for (var i = 0; i < a.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(a[i], b[i])) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> a && Equals(a);

    public override int GetHashCode()
    {
        if (_items is null) return 0;
        unchecked
        {
            var hash = 17;
            foreach (var item in _items) hash = hash * 31 + (item?.GetHashCode() ?? 0);
            return hash;
        }
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)(_items ?? Array.Empty<T>())).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static EquatableArray<T> From(IEnumerable<T> source) => new(source.ToArray());

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}
