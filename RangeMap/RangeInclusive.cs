// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace RangeMap;

public readonly record struct RangeInclusive<T>(T Start, T End) where T : IComparable<T>
{
    public bool Contains(T value) => Start.CompareTo(value) <= 0 && value.CompareTo(End) <= 0;

    public bool IsEmpty => Start.CompareTo(End) > 0;

    public override string ToString() => $"{Start}..={End}";

    public static implicit operator RangeInclusive<T>((T Start, T End) range) => new(range.Start, range.End);
}