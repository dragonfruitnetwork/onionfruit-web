// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Numerics;

namespace RangeMap.Tests;

internal static class TestHelpers
{
    public static (RangeInclusive<TKey> Range, TValue Value)[] ToTuples<TKey, TValue>(this RangeInclusiveMap<TKey, TValue> map) where TKey : IBinaryInteger<TKey>
    {
        return map.Select(entry => (entry.Key, entry.Value)).ToArray();
    }

    public static KeyValuePair<TKey, TValue> Entry<TKey, TValue>(TKey key, TValue value) => new(key, value);

    public static void AssertYields<T>(IEnumerator<T> enumerator, params T[] expected)
    {
        foreach (var item in expected)
        {
            Assert.That(enumerator.MoveNext(), Is.True, $"expected {item}");
            Assert.That(enumerator.Current, Is.EqualTo(item));
        }

        for (var i = 0; i < 3; i++)
        {
            Assert.That(enumerator.MoveNext(), Is.False, "expected the end of the sequence");
        }
    }
}

internal static class Permutations
{
    /// <summary>Every permutation of <paramref name="items"/>, by Heap's algorithm.</summary>
    public static IEnumerable<T[]> Of<T>(T[] items)
    {
        var a = (T[])items.Clone();
        var c = new int[a.Length];
        yield return (T[])a.Clone();

        var i = 0;
        while (i < a.Length)
        {
            if (c[i] < i)
            {
                var j = i % 2 == 0 ? 0 : c[i];
                (a[j], a[i]) = (a[i], a[j]);
                yield return (T[])a.Clone();

                c[i]++;
                i = 0;
            }
            else
            {
                c[i] = 0;
                i++;
            }
        }
    }
}

/// <summary>
/// A brute-force model of <see cref="RangeInclusiveMap{TKey, TValue}"/> over a window of
/// <c>Size</c> consecutive keys starting at <c>Origin</c>, storing the value of every key individually.
/// </summary>
internal sealed class DenseModel<TKey, TValue>(TKey origin, int size) where TKey : IBinaryInteger<TKey>
{
    private readonly bool[] _present = new bool[size];
    private readonly TValue[] _values = new TValue[size];

    public TKey Origin { get; } = origin;

    public int Size { get; } = size;

    public TKey Key(int offset) => Origin + TKey.CreateChecked(offset);

    private int Offset(TKey key) => int.CreateChecked(key - Origin);

    public void Insert(RangeInclusive<TKey> range, TValue value)
    {
        for (var i = Offset(range.Start); i <= Offset(range.End); i++)
        {
            _present[i] = true;
            _values[i] = value;
        }
    }

    public void Remove(RangeInclusive<TKey> range)
    {
        for (var i = Offset(range.Start); i <= Offset(range.End); i++)
        {
            _present[i] = false;
            _values[i] = default!;
        }
    }

    public bool TryGetValue(int offset, out TValue value)
    {
        value = _values[offset];
        return _present[offset];
    }

    // Runs of consecutive keys with equal values, i.e. what the map should contain.
    public List<(RangeInclusive<TKey> Range, TValue Value)> ToTuples()
    {
        var result = new List<(RangeInclusive<TKey>, TValue)>();
        var i = 0;
        while (i < Size)
        {
            if (!_present[i])
            {
                i++;
                continue;
            }

            var start = i;
            while (i + 1 < Size && _present[i + 1] && ValuesEqual(_values[i + 1], _values[start]))
            {
                i++;
            }

            result.Add((new RangeInclusive<TKey>(Key(start), Key(i)), _values[start]));
            i++;
        }

        return result;
    }

    // The largest runs of uncovered keys within the outer range.
    public List<RangeInclusive<TKey>> GetGaps(int outerStart, int outerEnd)
    {
        var result = new List<RangeInclusive<TKey>>();
        var i = outerStart;
        while (i <= outerEnd)
        {
            if (_present[i])
            {
                i++;
                continue;
            }

            var start = i;
            while (i + 1 <= outerEnd && !_present[i + 1])
            {
                i++;
            }

            result.Add(new RangeInclusive<TKey>(Key(start), Key(i)));
            i++;
        }

        return result;
    }

    private static bool ValuesEqual(TValue a, TValue b) => EqualityComparer<TValue>.Default.Equals(a, b);
}