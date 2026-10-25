// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace RangeMap;

/// <summary>
/// A map from inclusive ranges of integer keys (<c>Start..=End</c>) to values.
/// </summary>
public sealed class RangeInclusiveMap<TKey, TValue> :
    IReadOnlyCollection<KeyValuePair<RangeInclusive<TKey>, TValue>>,
    IEquatable<RangeInclusiveMap<TKey, TValue>>,
    IComparable<RangeInclusiveMap<TKey, TValue>>
    where TKey : IBinaryInteger<TKey>
{
    private readonly RangeTree<TKey, TValue> _tree;

    public RangeInclusiveMap()
    {
        _tree = new RangeTree<TKey, TValue>();
    }

    /// <summary>Creates a map by inserting each of the given entries in order.</summary>
    public RangeInclusiveMap(IEnumerable<KeyValuePair<RangeInclusive<TKey>, TValue>> entries) : this()
    {
        AddRange(entries);
    }

    private RangeInclusiveMap(RangeTree<TKey, TValue> tree)
    {
        _tree = tree;
    }

    public int Count => _tree.Count;

    public bool IsEmpty => _tree.Count == 0;

    /// <summary>Gets the value of the range containing <paramref name="key"/>.</summary>
    /// <exception cref="KeyNotFoundException">No range in the map contains the key.</exception>
    public TValue this[TKey key] => TryGetValue(key, out var value)
        ? value
        : throw new KeyNotFoundException($"No range in the map contains the key '{key}'.");

    public void Clear() => _tree.Clear();

    public RangeInclusiveMap<TKey, TValue> Clone() => new(_tree.Clone());

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return TryGetRange(key, out _, out value);
    }

    public bool TryGetRange(TKey key, out RangeInclusive<TKey> range, [MaybeNullWhen(false)] out TValue value)
    {
        // Only the last range starting at or before the key can contain it.
        if (_tree.TryFindLastAtOrBefore(key, out var candidate) && key <= candidate.End)
        {
            range = new RangeInclusive<TKey>(candidate.Start, candidate.End);
            value = candidate.Value;
            return true;
        }

        range = default;
        value = default;
        return false;
    }

    public bool ContainsKey(TKey key) => TryGetRange(key, out _, out _);

    /// <summary>Gets the first (lowest) range in the map and its value, if the map isn't empty.</summary>
    public bool TryGetFirst(out RangeInclusive<TKey> range, [MaybeNullWhen(false)] out TValue value)
    {
        return TryRead(_tree.TryGetFirst(out var first), first, out range, out value);
    }

    /// <summary>Gets the last (highest) range in the map and its value, if the map isn't empty.</summary>
    public bool TryGetLast(out RangeInclusive<TKey> range, [MaybeNullWhen(false)] out TValue value)
    {
        return TryRead(_tree.TryGetLast(out var last), last, out range, out value);
    }

    /// <summary>
    /// Gets the stored ranges that overlap <paramref name="range"/>, in order.
    /// Call <see cref="Enumerator.Reverse"/> on the result to get them in reverse order.
    /// </summary>
    public Enumerator GetOverlapping(RangeInclusive<TKey> range) => new(FindOverlapping(range), reverse: false);

    public bool Overlaps(RangeInclusive<TKey> range) => FindOverlapping(range).MoveNext();

    /// <summary>
    /// Gets the largest ranges within <paramref name="outerRange"/> that aren't covered by
    /// any stored range, in order.
    /// </summary>
    public GapEnumerator GetGaps(RangeInclusive<TKey> outerRange)
    {
        return new GapEnumerator(FindEndingAtOrAfter(outerRange.Start), outerRange);
    }

    public Enumerator GetEnumerator() => new(_tree.All(), reverse: false);

    public Enumerator Reverse() => new(_tree.All(), reverse: true);

    IEnumerator<KeyValuePair<RangeInclusive<TKey>, TValue>> IEnumerable<KeyValuePair<RangeInclusive<TKey>, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Maps every key in <paramref name="range"/> to <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    /// Stored ranges overlapping the new range are trimmed or removed to make room for it.
    /// The new range is merged with any stored ranges that overlap or are adjacent to it
    /// and have an equal value.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="range"/> is reversed (<c>Start &gt; End</c>).</exception>
    public void Insert(RangeInclusive<TKey> range, TValue value)
    {
        ThrowIfReversed(range);

        var start = range.Start;
        var end = range.End;

        // A stored range overlapping or immediately preceding the new range starts at or before it.
        // There can be two: one ending just before the new range, and one starting with it.
        // Handle the earlier one here - the later one is handled by the loop below.
        if (_tree.TryFindLastAtOrBefore(start, out var candidate))
        {
            var previous = candidate;
            if (RangeTree<TKey, TValue>.Retreat(ref previous) && Touches(previous.Start, previous.End, start, end))
            {
                MergeOrCut(previous, ref start, ref end, value);
            }
            else if (Touches(candidate.Start, candidate.End, start, end))
            {
                MergeOrCut(candidate, ref start, ref end, value);
            }
        }

        // Every other affected range starts within the new range or immediately after it.
        // The search is bounded by the original end: merging can extend `end`, but only up
        // to the end of a stored range, which is never followed by a range it could merge with.
        var originalEnd = end;
        while (_tree.TryFindFirstAtOrAfter(start, out var stored))
        {
            if (stored.Start > originalEnd)
            {
                // A range starts after originalEnd, so originalEnd + 1 can't overflow.
                // A range starting there is adjacent, so is only affected if it can be merged.
                if (stored.Start != AddOne(originalEnd) || !ValuesEqual(stored.Value, value))
                {
                    break;
                }
            }

            MergeOrCut(stored, ref start, ref end, value);
        }

        _tree.Insert(start, end, value);
    }

    /// <summary>Inserts a range; the same as <see cref="Insert"/>. Enables collection initializers.</summary>
    public void Add(RangeInclusive<TKey> range, TValue value) => Insert(range, value);

    /// <summary>Inserts each of the given entries in order.</summary>
    public void AddRange(IEnumerable<KeyValuePair<RangeInclusive<TKey>, TValue>> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        foreach (var (range, value) in entries)
        {
            Insert(range, value);
        }
    }

    /// <summary>
    /// Unmaps every key in <paramref name="range"/>, trimming or removing any stored ranges that overlap it.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="range"/> is reversed (<c>Start &gt; End</c>).</exception>
    public void Remove(RangeInclusive<TKey> range)
    {
        ThrowIfReversed(range);

        // A stored range starting at or before the removed range may overlap its start...
        if (_tree.TryFindLastAtOrBefore(range.Start, out var stored) && stored.End >= range.Start)
        {
            Cut(stored, range.Start, range.End);
        }

        // ...and every other overlapping range starts within it.
        while (_tree.TryFindFirstAfter(range.Start, out stored) && stored.Start <= range.End)
        {
            Cut(stored, range.Start, range.End);
        }
    }

    // Resolves a stored range that touches the range being inserted. If it has the same value
    // the new range absorbs it; otherwise the part overlapping the new range is cut out of it.
    private void MergeOrCut(RangeTree<TKey, TValue>.Cursor stored, ref TKey start, ref TKey end, TValue value)
    {
        if (ValuesEqual(stored.Value, value))
        {
            start = TKey.Min(start, stored.Start);
            end = TKey.Max(end, stored.End);
            _tree.Remove(stored.Start);
            return;
        }

        // Ranges that are merely adjacent can both stay as they are.
        if (Overlaps(start, end, stored.Start, stored.End))
        {
            Cut(stored, start, end);
        }
    }

    // Removes the keys start..=end from a stored range that overlaps them,
    // leaving up to two pieces of it on either side.
    private void Cut(RangeTree<TKey, TValue>.Cursor stored, TKey start, TKey end)
    {
        var storedStart = stored.Start;
        var storedEnd = stored.End;
        var storedValue = stored.Value;

        if (storedStart < start)
        {
            // The left piece keeps the same start, so the stored range can be shortened in place.
            _tree.SetEnd(stored, SubOne(start));
        }
        else
        {
            _tree.Remove(storedStart);
        }

        if (storedEnd > end)
        {
            _tree.Insert(AddOne(end), storedEnd, storedValue);
        }
    }

    // Finds the first stored range ending at or after `key`. Stored ranges don't overlap, so it's
    // either the last range starting at or before the key (if that reaches the key), or the next one.
    private bool TryFindFirstEndingAtOrAfter(TKey key, out RangeTree<TKey, TValue>.Cursor cursor)
    {
        if (!_tree.TryFindLastAtOrBefore(key, out cursor))
        {
            return _tree.TryGetFirst(out cursor);
        }

        return cursor.End >= key || RangeTree<TKey, TValue>.Advance(ref cursor);
    }

    private RangeTree<TKey, TValue>.EntryRange FindOverlapping(RangeInclusive<TKey> range)
    {
        if (!TryFindFirstEndingAtOrAfter(range.Start, out var first) || first.Start > range.End)
        {
            return default;
        }

        _tree.TryFindLastAtOrBefore(range.End, out var last);
        return new RangeTree<TKey, TValue>.EntryRange(_tree, first, last);
    }

    private RangeTree<TKey, TValue>.EntryRange FindEndingAtOrAfter(TKey key)
    {
        if (!TryFindFirstEndingAtOrAfter(key, out var first) || !_tree.TryGetLast(out var last))
        {
            return default;
        }

        return new RangeTree<TKey, TValue>.EntryRange(_tree, first, last);
    }

    private static bool TryRead(
        bool found,
        RangeTree<TKey, TValue>.Cursor entry,
        out RangeInclusive<TKey> range,
        [MaybeNullWhen(false)] out TValue value)
    {
        if (!found)
        {
            range = default;
            value = default;
            return false;
        }

        range = new RangeInclusive<TKey>(entry.Start, entry.End);
        value = entry.Value;
        return true;
    }

    private static bool Overlaps(TKey start1, TKey end1, TKey start2, TKey end2)
    {
        return TKey.Max(start1, start2) <= TKey.Min(end1, end2);
    }

    // Whether two ranges overlap or are adjacent.
    private static bool Touches(TKey start1, TKey end1, TKey start2, TKey end2)
    {
        var maxStart = TKey.Max(start1, start2);
        var minEnd = TKey.Min(end1, end2);

        // Only step past minEnd when the ranges don't overlap, as then it can't overflow.
        return maxStart <= minEnd || AddOne(minEnd) == maxStart;
    }

    private static TKey AddOne(TKey value) => checked(value + TKey.One);

    private static TKey SubOne(TKey value) => checked(value - TKey.One);

    private static bool ValuesEqual(TValue a, TValue b) => EqualityComparer<TValue>.Default.Equals(a, b);

    private static KeyValuePair<RangeInclusive<TKey>, TValue> ToEntry(RangeTree<TKey, TValue>.Cursor cursor)
    {
        var range = new RangeInclusive<TKey>(cursor.Start, cursor.End);
        return new KeyValuePair<RangeInclusive<TKey>, TValue>(range, cursor.Value);
    }

    private static void ThrowIfReversed(RangeInclusive<TKey> range)
    {
        if (range.Start > range.End)
        {
            throw new ArgumentException($"Range start can not be after range end, but got {range}.", nameof(range));
        }
    }

    public bool Equals(RangeInclusiveMap<TKey, TValue>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Count != other.Count)
        {
            return false;
        }

        var mine = _tree.All();
        var theirs = other._tree.All();
        while (mine.MoveNext() && theirs.MoveNext())
        {
            var a = mine.Current;
            var b = theirs.Current;
            if (a.Start != b.Start || a.End != b.End || !ValuesEqual(a.Value, b.Value))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is RangeInclusiveMap<TKey, TValue> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Count);

        var entries = _tree.All();
        while (entries.MoveNext())
        {
            var entry = entries.Current;
            hash.Add(entry.Start);
            hash.Add(entry.End);
            hash.Add(entry.Value);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Compares maps by their entries in order, comparing each entry by start, then end, then value.
    /// A map that is a prefix of another comes before it.
    /// </summary>
    /// <exception cref="ArgumentException"><typeparamref name="TValue"/> isn't comparable.</exception>
    public int CompareTo(RangeInclusiveMap<TKey, TValue>? other)
    {
        if (other is null)
        {
            return 1;
        }

        var values = Comparer<TValue>.Default;
        var mine = _tree.All();
        var theirs = other._tree.All();
        while (true)
        {
            var hasMine = mine.MoveNext();
            var hasTheirs = theirs.MoveNext();
            if (!hasMine || !hasTheirs)
            {
                return hasMine.CompareTo(hasTheirs);
            }

            var a = mine.Current;
            var b = theirs.Current;
            var comparison = a.Start.CompareTo(b.Start);
            if (comparison == 0)
            {
                comparison = a.End.CompareTo(b.End);
            }

            if (comparison == 0)
            {
                comparison = values.Compare(a.Value, b.Value);
            }

            if (comparison != 0)
            {
                return comparison;
            }
        }
    }

    public static bool operator ==(RangeInclusiveMap<TKey, TValue>? left, RangeInclusiveMap<TKey, TValue>? right)
    {
        return left is null ? right is null : left.Equals(right);
    }

    public static bool operator !=(RangeInclusiveMap<TKey, TValue>? left, RangeInclusiveMap<TKey, TValue>? right)
    {
        return !(left == right);
    }

    /// <summary>Formats the map as its entries, e.g. <c>{2..=5: a, 7..=8: b}</c>.</summary>
    public override string ToString()
    {
        var builder = new StringBuilder("{");
        var first = true;
        foreach (var (range, value) in this)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            first = false;
            builder.Append(range).Append(": ").Append(value);
        }

        return builder.Append('}').ToString();
    }

    internal void ValidateInvariants()
    {
        _tree.Validate();

        var entries = _tree.All();
        var hasPrevious = false;
        TKey previousEnd = default!;
        TValue previousValue = default!;
        while (entries.MoveNext())
        {
            var entry = entries.Current;
            if (entry.Start > entry.End)
            {
                throw new InvalidOperationException($"Reversed range {entry.Start}..={entry.End} stored.");
            }

            if (hasPrevious)
            {
                if (previousEnd >= entry.Start)
                {
                    throw new InvalidOperationException(
                        $"Range starting at {entry.Start} overlaps the previous range.");
                }

                if (AddOne(previousEnd) == entry.Start && ValuesEqual(previousValue, entry.Value))
                {
                    throw new InvalidOperationException(
                        $"Range starting at {entry.Start} wasn't merged with the previous range.");
                }
            }

            hasPrevious = true;
            previousEnd = entry.End;
            previousValue = entry.Value;
        }
    }

    /// <summary>
    /// Enumerates ranges of a <see cref="RangeInclusiveMap{TKey, TValue}"/> and their values.
    /// </summary>
    /// <remarks>
    /// This is both the enumerable and its enumerator, so it can be used directly in <c>foreach</c>
    /// and LINQ, and reversed with <see cref="Reverse"/> before enumerating.
    /// </remarks>
    public struct Enumerator :
        IEnumerable<KeyValuePair<RangeInclusive<TKey>, TValue>>,
        IEnumerator<KeyValuePair<RangeInclusive<TKey>, TValue>>
    {
        private RangeTree<TKey, TValue>.EntryRange _entries;
        private readonly bool _reverse;

        internal Enumerator(RangeTree<TKey, TValue>.EntryRange entries, bool reverse)
        {
            _entries = entries;
            _reverse = reverse;
        }

        public readonly KeyValuePair<RangeInclusive<TKey>, TValue> Current => ToEntry(_entries.Current);

        object IEnumerator.Current => Current;

        public readonly Enumerator Reverse() => new(_entries, !_reverse);

        public readonly Enumerator GetEnumerator() => this;

        public bool MoveNext() => _reverse ? _entries.MoveNextBack() : _entries.MoveNext();

        IEnumerator<KeyValuePair<RangeInclusive<TKey>, TValue>>
            IEnumerable<KeyValuePair<RangeInclusive<TKey>, TValue>>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() => this;

        void IEnumerator.Reset() => throw new NotSupportedException();

        public readonly void Dispose()
        {
        }
    }

    /// <summary>
    /// Enumerates the largest ranges within an outer range that aren't covered by
    /// any range in a <see cref="RangeInclusiveMap{TKey, TValue}"/>.
    /// </summary>
    /// <remarks>
    /// This is both the enumerable and its enumerator, so it can be used directly in <c>foreach</c> and LINQ.
    /// </remarks>
    public struct GapEnumerator : IEnumerable<RangeInclusive<TKey>>, IEnumerator<RangeInclusive<TKey>>
    {
        // Stored ranges from the first one ending at or after the start of the outer range.
        private RangeTree<TKey, TValue>.EntryRange _stored;

        private readonly TKey _outerEnd;

        // The first key that may start the next gap.
        private TKey _gapStart;
        private bool _done;

        internal GapEnumerator(RangeTree<TKey, TValue>.EntryRange stored, RangeInclusive<TKey> outerRange)
        {
            _stored = stored;
            _outerEnd = outerRange.End;
            _gapStart = outerRange.Start;
            _done = outerRange.Start > outerRange.End;
            Current = default;
        }

        public RangeInclusive<TKey> Current { get; private set; }

        object IEnumerator.Current => Current;

        public readonly GapEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            if (_done)
            {
                return false;
            }

            while (_stored.MoveNext())
            {
                var stored = _stored.Current;
                if (stored.Start > _outerEnd)
                {
                    break;
                }

                if (_gapStart < stored.Start)
                {
                    // The gap ends just before this stored range.
                    Current = new RangeInclusive<TKey>(_gapStart, SubOne(stored.Start));
                    SkipPast(stored.End);
                    return true;
                }

                // This stored range covers the gap start.
                SkipPast(stored.End);
                if (_done)
                {
                    return false;
                }
            }

            // There are no more stored ranges within the outer range,
            // so the rest of it is the last gap.
            _done = true;
            Current = new RangeInclusive<TKey>(_gapStart, _outerEnd);
            return true;
        }

        // Moves the gap start past the end of a stored range, finishing at the end of the outer range
        // (which also avoids overflowing when the stored range ends at the maximum key).
        private void SkipPast(TKey storedEnd)
        {
            if (storedEnd >= _outerEnd)
            {
                _done = true;
            }
            else
            {
                _gapStart = AddOne(storedEnd);
            }
        }

        IEnumerator<RangeInclusive<TKey>> IEnumerable<RangeInclusive<TKey>>.GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        void IEnumerator.Reset() => throw new NotSupportedException();

        public readonly void Dispose()
        {
        }
    }
}