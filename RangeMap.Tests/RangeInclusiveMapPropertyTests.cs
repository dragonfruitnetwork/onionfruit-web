// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Numerics;
using CsCheck;

namespace RangeMap.Tests;

[TestFixture]
public class RangeInclusiveMapPropertyTests
{
    private const int Iterations = 1_000;

    #region Generators

    private static readonly Gen<uint> UIntKey = Gen.Int[0, 3].Select(Gen.Int[0, 1_000],
        Gen.Int,
        (kind, small, any) => kind switch
        {
            0 => (uint)small,
            1 => uint.MaxValue - (uint)small,
            2 => 1_000_000u + (uint)small,
            _ => unchecked((uint)any),
        });

    private static readonly Gen<UInt128> UInt128Key = Gen.Int[0, 3].Select(Gen.Int[0, 1_000],
        Gen.ULong,
        Gen.ULong,
        (kind, small, upper, lower) => kind switch
        {
            0 => (uint)small,
            1 => UInt128.MaxValue - (uint)small,
            2 => UInt128.MaxValue / 2 + (uint)small,
            _ => new UInt128(upper, lower),
        });

    // A small number of values, so that merging happens often.
    private static readonly Gen<string> Value = Gen.Int[0, 3].Select(i => ((char)('a' + i)).ToString());

    private static Gen<RangeInclusive<TKey>> RangeOf<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
    {
        return key.Select(key, (a, b) => a <= b ? new RangeInclusive<TKey>(a, b) : new RangeInclusive<TKey>(b, a));
    }

    private static Gen<(RangeInclusive<TKey> Range, string Value)[]> EntriesOf<TKey>(Gen<TKey> key)
        where TKey : IBinaryInteger<TKey>
    {
        return RangeOf(key).Select(Value, (range, value) => (range, value)).Array[0, 40];
    }

    private static RangeInclusiveMap<TKey, string> Build<TKey>((RangeInclusive<TKey> Range, string Value)[] entries)
        where TKey : IBinaryInteger<TKey>
    {
        var map = new RangeInclusiveMap<TKey, string>();
        foreach (var (range, value) in entries)
        {
            map.Insert(range, value);
        }

        return map;
    }

    private static Gen<RangeInclusiveMap<TKey, string>> MapOf<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
    {
        return EntriesOf(key).Select(entries => Build(entries));
    }

    // Two maps, which are often equal, for testing equality, hashing, and ordering.
    private static Gen<(RangeInclusiveMap<TKey, string> Left, RangeInclusiveMap<TKey, string> Right)> PairOf<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
    {
        return MapOf(key).Select(MapOf(key), Gen.Bool, (left, right, same) => (left, same ? left.Clone() : right));
    }

    #endregion

    #region General properties

    [Test]
    public void CountMatchesEnumerationAndClearEmpties()
    {
        static void Verify<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
        {
            MapOf(key).Sample(map =>
            {
                Assert.That(map.Count, Is.EqualTo(map.Count()));
                Assert.That(map.IsEmpty, Is.EqualTo(map.Count == 0));

                map.Clear();
                Assert.That(map.Count, Is.Zero);
                Assert.That(map.IsEmpty, Is.True);
                Assert.That(map.Count(), Is.Zero);
            }, iter: Iterations);
        }

        Verify(UIntKey);
        Verify(UInt128Key);
    }

    [Test]
    public void TryGetFirstReturnsLowestRange()
    {
        static void Verify<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
        {
            MapOf(key).Sample(map =>
            {
                var found = map.TryGetFirst(out var range, out var value);
                Assert.That(found, Is.EqualTo(!map.IsEmpty));
                if (found)
                {
                    var expected = map.MinBy(kv => kv.Key.Start);
                    Assert.That((range, value), Is.EqualTo((expected.Key, expected.Value)));
                }
            }, iter: Iterations);
        }

        Verify(UIntKey);
        Verify(UInt128Key);
    }

    [Test]
    public void TryGetLastReturnsHighestRange()
    {
        static void Verify<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
        {
            MapOf(key).Sample(map =>
            {
                var found = map.TryGetLast(out var range, out var value);
                Assert.That(found, Is.EqualTo(!map.IsEmpty));
                if (found)
                {
                    var expected = map.MaxBy(kv => kv.Key.End);
                    Assert.That((range, value), Is.EqualTo((expected.Key, expected.Value)));
                }
            }, iter: Iterations);
        }

        Verify(UIntKey);
        Verify(UInt128Key);
    }

    [Test]
    public void ReverseEnumeratesInOppositeOrder()
    {
        static void Verify<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
        {
            MapOf(key).Sample(map =>
            {
                var forward = map.ToList();
                var backward = map.Reverse().ToList();
                backward.Reverse();
                Assert.That(backward, Is.EqualTo(forward));
            }, iter: Iterations);
        }

        Verify(UIntKey);
        Verify(UInt128Key);
    }

    [Test]
    public void GetOverlappingMatchesFilterInBothDirections()
    {
        static void Verify<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
        {
            MapOf(key).Select(RangeOf(key)).Sample((map, range) =>
            {
                var forward = map.GetOverlapping(range).ToList();
                var backward = map.GetOverlapping(range).Reverse().ToList();
                backward.Reverse();
                Assert.That(backward, Is.EqualTo(forward));

                // And it's exactly the stored ranges that overlap the query.
                var expected = map.Where(kv => kv.Key.Start <= range.End && range.Start <= kv.Key.End).ToList();
                Assert.That(forward, Is.EqualTo(expected));
                Assert.That(map.Overlaps(range), Is.EqualTo(expected.Count > 0));
            }, iter: Iterations);
        }

        Verify(UIntKey);
        Verify(UInt128Key);
    }

    [Test]
    public void LookupReturnsValueOfLastInsertedRange()
    {
        RangeOf(Gen.Byte).Select(Value, (range, value) => (range, value)).Array[0, 40].Sample(entries =>
        {
            var map = Build(entries);
            map.ValidateInvariants();

            for (var value = 0; value <= byte.MaxValue; value++)
            {
                var key = (byte)value;

                // The value of a key is that of the last range inserted that contains it.
                string? expected = null;
                for (var i = entries.Length - 1; i >= 0; i--)
                {
                    if (entries[i].range.Contains(key))
                    {
                        expected = entries[i].value;
                        break;
                    }
                }

                Assert.That(map.TryGetValue(key, out var actual) ? actual : null, Is.EqualTo(expected));
            }
        }, iter: Iterations);
    }

    [Test]
    public void EqualMapsHaveEqualHashCodes()
    {
        static void Verify<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
        {
            PairOf(key).Sample((left, right) =>
            {
                if (left == right)
                {
                    Assert.That(
                        left.GetHashCode(),
                        Is.EqualTo(right.GetHashCode()),
                        "if two values are equal, their hash must be equal");
                }

                // If the hashes are equal the values might not be the same (collision).
                if (left.GetHashCode() != right.GetHashCode())
                {
                    Assert.That(left != right, Is.True, "if two values' hashes are not equal, they must not be equal");
                }
            }, iter: Iterations);
        }

        Verify(UIntKey);
        Verify(UInt128Key);
    }

    [Test]
    public void CompareToAgreesWithEquality()
    {
        static void Verify<TKey>(Gen<TKey> key) where TKey : IBinaryInteger<TKey>
        {
            PairOf(key).Sample((left, right) =>
            {
                Assert.That(left == right, Is.EqualTo(left.CompareTo(right) == 0), "ordering and equality must match");
                Assert.That(
                    Math.Sign(left.CompareTo(right)),
                    Is.EqualTo(-Math.Sign(right.CompareTo(left))),
                    "ordering must be antisymmetric");
            }, iter: Iterations);
        }

        Verify(UIntKey);
        Verify(UInt128Key);
    }

    #endregion

    #region Random sequences of operations on byte keys

    private readonly record struct ByteOp(bool Insert, byte Start, byte End, byte Value)
    {
        public void Apply(RangeInclusiveMap<byte, byte> map)
        {
            // Skip reversed ranges rather than throwing.
            if (Start > End)
            {
                return;
            }

            if (Insert)
            {
                map.Insert(new RangeInclusive<byte>(Start, End), Value);
            }
            else
            {
                map.Remove(new RangeInclusive<byte>(Start, End));
            }
        }
    }

    private static readonly Gen<ByteOp[]> ByteOps = Gen.Bool.Select(Gen.Byte, Gen.Byte, Gen.Byte[0, 3], (insert, start, end, value) => new ByteOp(insert, start, end, value)).Array[0, 50];

    private static RangeInclusiveMap<byte, byte> Apply(ByteOp[] ops)
    {
        var map = new RangeInclusiveMap<byte, byte>();
        foreach (var op in ops)
        {
            op.Apply(map);
        }

        return map;
    }

    [Test]
    public void AdjacentRangesWithEqualValuesAreAlwaysMerged()
    {
        ByteOps.Sample(ops =>
        {
            var entries = Apply(ops).ToList();
            for (var i = 0; i + 1 < entries.Count; i++)
            {
                var (range, value) = entries[i];
                var (nextRange, nextValue) = entries[i + 1];
                var adjacentWithSameValue = range.End == nextRange.Start - 1 && value == nextValue;
                Assert.That(adjacentWithSameValue, Is.False, $"{range} and {nextRange} were not merged");
            }
        }, iter: Iterations);
    }

    [Test]
    public void GapsAndOverlappingRangesTileTheOuterRange()
    {
        // Larger margins than these are too far away from boundary conditions to be interesting.
        var outerStart = Gen.OneOf<byte>(0, 1, 2, 3, 100, 101, 102, 103);
        var outerEnd = Gen.OneOf<byte>(100, 101, 102, 103, 252, 253, 254, 255);

        ByteOps.Select(outerStart, outerEnd).Sample((ops, start, end) =>
        {
            var map = Apply(ops);
            var outerRange = new RangeInclusive<byte>(start, end);

            // Check that the combination of gaps and keys fills the entire outer range.
            var keys = map.GetOverlapping(outerRange)
                .Select(kv => new RangeInclusive<byte>(Math.Max(kv.Key.Start, start), Math.Min(kv.Key.End, end)))
                .Where(range => !range.IsEmpty)
                .Concat(map.GetGaps(outerRange))
                .OrderBy(range => range.Start)
                .ToList();

            if (outerRange.IsEmpty)
            {
                // There should be no gaps or keys returned if the outer range is empty,
                // because empty ranges cover no values.
                Assert.That(keys, Is.Empty);
                return;
            }

            // Gaps and keys combined should span whole outer range.
            Assert.That(keys[0].Start, Is.EqualTo(start));
            Assert.That(keys[^1].End, Is.EqualTo(end));

            // Each gap or range should start just after the previous one ends.
            for (var i = 0; i + 1 < keys.Count; i++)
            {
                Assert.That(keys[i].End + 1, Is.EqualTo(keys[i + 1].Start));
            }
        }, iter: Iterations);
    }

    #endregion

    #region Model-based tests

    private const int WindowSize = 256;

    private readonly record struct Op(bool Insert, int Start, int End, int Value);

    // Offsets within the window; insert two-thirds of the time.
    private static readonly Gen<Op[]> Ops = Gen.Int[0, 2].Select(Gen.Int[0, WindowSize - 1],
            Gen.Int[0, WindowSize - 1],
            Gen.Int[0, 3],
            (kind, a, b, value) => new Op(kind != 0, Math.Min(a, b), Math.Max(a, b), value))
        .Array[0, 60];

    private static readonly Gen<(Op[] Ops, int QueryStart, int QueryEnd)> OpsAndQuery =
        Ops.Select(Gen.Int[0, WindowSize - 1],
            Gen.Int[0, WindowSize - 1],
            (ops, a, b) => (ops, Math.Min(a, b), Math.Max(a, b)));

    private static void CheckAgainstModel<TKey>(TKey origin, Op[] ops, int queryStart, int queryEnd) where TKey : IBinaryInteger<TKey>
    {
        var map = new RangeInclusiveMap<TKey, int>();
        var model = new DenseModel<TKey, int>(origin, WindowSize);

        foreach (var op in ops)
        {
            var range = new RangeInclusive<TKey>(model.Key(op.Start), model.Key(op.End));
            if (op.Insert)
            {
                map.Insert(range, op.Value);
                model.Insert(range, op.Value);
            }
            else
            {
                map.Remove(range);
                model.Remove(range);
            }

            map.ValidateInvariants();
            Assert.That(map.ToTuples(), Is.EqualTo(model.ToTuples()), $"after {op}");
        }

        // Lookups of every key in the window.
        for (var offset = 0; offset < WindowSize; offset++)
        {
            var found = map.TryGetValue(model.Key(offset), out var actual);
            var expectedFound = model.TryGetValue(offset, out var expected);
            Assert.That((found, actual), Is.EqualTo((expectedFound, expected)), $"lookup of offset {offset}");
        }

        // Gaps and overlapping ranges for the query.
        var query = new RangeInclusive<TKey>(model.Key(queryStart), model.Key(queryEnd));
        Assert.That(map.GetGaps(query), Is.EqualTo(model.GetGaps(queryStart, queryEnd)), $"gaps in {query}");

        var overlapping = model.ToTuples().Where(entry => entry.Range.Start <= query.End && query.Start <= entry.Range.End).ToList();
        var forward = map.GetOverlapping(query).Select(kv => (kv.Key, kv.Value));
        Assert.That(forward, Is.EqualTo(overlapping), $"ranges overlapping {query}");

        var backward = map.GetOverlapping(query).Reverse().Select(kv => (kv.Key, kv.Value)).Reverse();
        Assert.That(backward, Is.EqualTo(overlapping), $"ranges overlapping {query}, in reverse");
    }

    [TestCase(0u)]
    [TestCase(1_000_000u)]
    [TestCase(4_294_967_040u)] // uint.MaxValue - (WindowSize - 1)
    public void UIntMatchesModel(uint origin)
    {
        OpsAndQuery.Sample((ops, queryStart, queryEnd) => CheckAgainstModel(origin, ops, queryStart, queryEnd), iter: Iterations);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void UInt128MatchesModel(int window)
    {
        var origin = window switch
        {
            0 => UInt128.Zero,
            1 => UInt128.MaxValue / 2,
            _ => UInt128.MaxValue - (uint)(WindowSize - 1),
        };

        OpsAndQuery.Sample((ops, queryStart, queryEnd) => CheckAgainstModel(origin, ops, queryStart, queryEnd), iter: Iterations);
    }

    [Test]
    public void ByteMatchesModelAcrossTheWholeDomain()
    {
        OpsAndQuery.Sample((ops, queryStart, queryEnd) => CheckAgainstModel(byte.MinValue, ops, queryStart, queryEnd), iter: Iterations);
    }

    #endregion
}