using static RangeMap.Tests.TestHelpers;

namespace RangeMap.Tests;

[TestFixture]
public class RangeInclusiveMapTests
{
    private static RangeInclusive<uint> UIntRange(uint start, uint end) => new(start, end);
    private static RangeInclusive<byte> ByteRange(byte start, byte end) => new(start, end);

    private static void AssertContents<TValue>(RangeInclusiveMap<uint, TValue> map, params (RangeInclusive<uint>, TValue)[] expected)
    {
        map.ValidateInvariants();
        Assert.That(map.ToTuples(), Is.EqualTo(expected));
    }

    #region Construction and insertion

    [Test]
    public void CollectionInitializerInsertsEntries()
    {
        var map = new RangeInclusiveMap<int, string>();
        map.Insert((0, 100), "hello");
        map.Insert((200, 300), "world");
        var expected = new RangeInclusiveMap<int, string>
        {
            { (0, 100), "hello" },
            { (200, 300), "world" },
        };
        Assert.That(map, Is.EqualTo(expected));
    }

    [Test]
    public void ConstructorInsertsEntriesInOrder()
    {
        Assert.That(new RangeInclusiveMap<long, long>([]), Is.EqualTo(new RangeInclusiveMap<long, long>()));
        Assert.That(
            new RangeInclusiveMap<int, string> { { (0, 100), "abc" }, { (100, 200), "def" }, { (200, 300), "ghi" } },
            Is.EqualTo(new RangeInclusiveMap<int, string>([
                Entry(new RangeInclusive<int>(0, 100), "abc"),
                Entry(new RangeInclusive<int>(100, 200), "def"),
                Entry(new RangeInclusive<int>(200, 300), "ghi"),
            ])));
    }

    [Test]
    public void EmptyMapIsEmpty()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        AssertContents(map);
        Assert.That(map.IsEmpty, Is.True);
    }

    [Test]
    public void InsertIntoEmptyMap()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(0, 50), false);
        AssertContents(map, (UIntRange(0, 50), false));
    }

    [Test]
    public void InsertSameValueImmediatelyAfterStoredMerges()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(1, 3), false);
        map.Insert(UIntRange(4, 6), false);
        AssertContents(map, (UIntRange(1, 6), false));
    }

    [Test]
    public void InsertDifferentValueImmediatelyAfterStoredKeepsBoth()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(1, 3), false);
        map.Insert(UIntRange(4, 6), true);
        AssertContents(map, (UIntRange(1, 3), false), (UIntRange(4, 6), true));
    }

    [Test]
    public void InsertSameValueOverlappingEndOfStoredMerges()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(1, 4), false);
        map.Insert(UIntRange(4, 6), false);
        AssertContents(map, (UIntRange(1, 6), false));
    }

    [Test]
    public void InsertDifferentValueOverlappingEndOfStoredTrimsIt()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(1, 3), false);
        map.Insert(UIntRange(3, 5), true);
        AssertContents(map, (UIntRange(1, 2), false), (UIntRange(3, 5), true));
    }

    [Test]
    public void InsertSameValueImmediatelyBeforeStoredMerges()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(3, 5), false);
        map.Insert(UIntRange(1, 2), false);
        AssertContents(map, (UIntRange(1, 5), false));
    }

    [Test]
    public void InsertDifferentValueImmediatelyBeforeStoredKeepsBoth()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(3, 5), true);
        map.Insert(UIntRange(1, 2), false);
        AssertContents(map, (UIntRange(1, 2), false), (UIntRange(3, 5), true));
    }

    [Test]
    public void InsertSameValueInsideStoredChangesNothing()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(1, 5), false);
        map.Insert(UIntRange(2, 4), false);
        AssertContents(map, (UIntRange(1, 5), false));
    }

    [Test]
    public void InsertDifferentValueInsideStoredSplitsIt()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(1, 5), true);
        map.Insert(UIntRange(2, 4), false);
        AssertContents(map, (UIntRange(1, 1), true), (UIntRange(2, 4), false), (UIntRange(5, 5), true));
    }

    [Test]
    public void ReplacingValueNextToEqualRangeMerges()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(1, 3), false);
        map.Insert(UIntRange(4, 6), true);
        map.Insert(UIntRange(4, 6), false);
        AssertContents(map, (UIntRange(1, 6), false));
    }

    [Test]
    // Inserts a set of touching and overlapping ranges in every possible order.
    public void EveryInsertOrderMatchesModel()
    {
        (RangeInclusive<uint>, bool)[] rangesWithValues =
        [
            (UIntRange(2, 3), false),
            // A duplicate range
            (UIntRange(2, 3), false),
            // Almost a duplicate, but with a different value
            (UIntRange(2, 3), true),
            // A few small ranges, some of them overlapping others,
            // some of them touching others
            (UIntRange(3, 5), true),
            (UIntRange(4, 6), true),
            (UIntRange(6, 7), true),
            // A large range
            (UIntRange(2, 6), true),
        ];

        foreach (var permutation in Permutations.Of(rangesWithValues))
        {
            var map = new RangeInclusiveMap<uint, bool>();
            var model = new DenseModel<uint, bool>(0, 16);

            foreach (var (range, value) in permutation)
            {
                map.Insert(range, value);
                model.Insert(range, value);

                map.ValidateInvariants();
                Assert.That(map.ToTuples(), Is.EqualTo(model.ToTuples()));
            }
        }
    }

    #endregion

    #region Lookup tests

    [Test]
    public void TryGetValueAndIndexer()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(0, 50), false);
        Assert.That(map.TryGetValue(50, out var value) && !value, Is.True);
        Assert.That(map.TryGetValue(51, out _), Is.False);
        Assert.That(map[50], Is.False);
        Assert.Throws<KeyNotFoundException>(() => _ = map[51]);
    }

    [Test]
    public void TryGetRangeReturnsContainingRange()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(0, 50), false);
        Assert.That(map.TryGetRange(50, out var range, out var value), Is.True);
        Assert.That((range, value), Is.EqualTo((UIntRange(0, 50), false)));
        Assert.That(map.TryGetRange(51, out _, out _), Is.False);
    }

    #endregion

    #region Removal tests

    [Test]
    public void RemoveFromEmptyMap()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Remove(UIntRange(0, 50));
        AssertContents(map);
    }

    [Test]
    public void RemoveBeforeStoredChangesNothing()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(25, 75), false);
        map.Remove(UIntRange(0, 24));
        AssertContents(map, (UIntRange(25, 75), false));
    }

    [Test]
    public void RemoveAfterStoredChangesNothing()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(25, 75), false);
        map.Remove(UIntRange(76, 100));
        AssertContents(map, (UIntRange(25, 75), false));
    }

    [Test]
    public void RemoveOverlappingStartOfStoredTrimsIt()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(25, 75), false);
        map.Remove(UIntRange(0, 25));
        AssertContents(map, (UIntRange(26, 75), false));
    }

    [Test]
    public void RemoveMiddleOfStoredSplitsIt()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(25, 75), false);
        map.Remove(UIntRange(30, 70));
        AssertContents(map, (UIntRange(25, 29), false), (UIntRange(71, 75), false));
    }

    [Test]
    public void RemoveOverlappingEndOfStoredTrimsIt()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(25, 75), false);
        map.Remove(UIntRange(75, 100));
        AssertContents(map, (UIntRange(25, 74), false));
    }

    [Test]
    public void RemoveExactlyStoredRemovesIt()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(25, 75), false);
        map.Remove(UIntRange(25, 75));
        AssertContents(map);
    }

    [Test]
    public void RemoveSupersetOfStoredRemovesIt()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        map.Insert(UIntRange(25, 75), false);
        map.Remove(UIntRange(0, 100));
        AssertContents(map);
    }

    [Test]
    public void ReversedRangesAreRejected()
    {
        var map = new RangeInclusiveMap<uint, bool>();
        Assert.Throws<ArgumentException>(() => map.Insert(UIntRange(4, 3), false));
        Assert.Throws<ArgumentException>(() => map.Remove(UIntRange(4, 3)));
        // But single-value ranges are fine.
        map.Insert(UIntRange(3, 3), false);
        AssertContents(map, (UIntRange(3, 3), false));
    }

    #endregion

    #region Key domain extremes

    // Key arithmetic is checked, so any overflow in these tests would throw.

    [Test]
    public void NoOverflowAtKeyDomainExtremes()
    {
        var map = new RangeInclusiveMap<byte, bool>();
        map.Insert(ByteRange(0, 255), false);
        map.Insert(ByteRange(0, 10), true);
        map.Insert(ByteRange(245, 255), true);
        map.Remove(ByteRange(0, 5));
        map.Remove(ByteRange(0, 5));
        map.Remove(ByteRange(250, 255));
        map.Remove(ByteRange(250, 255));
        map.Insert(ByteRange(0, 255), true);
        map.Remove(ByteRange(1, 254));
        map.Insert(ByteRange(254, 254), true);
        map.Insert(ByteRange(255, 255), true);
        map.Insert(ByteRange(255, 255), false);
        map.Insert(ByteRange(0, 0), false);
        map.Insert(ByteRange(1, 1), true);
        map.Insert(ByteRange(0, 0), true);
        map.ValidateInvariants();
        Assert.That(map.ToTuples(), Is.EqualTo(new[] { (ByteRange(0, 1), true), (ByteRange(254, 254), true), (ByteRange(255, 255), false) }));
    }

    #endregion

    #region Gaps tests

    [Test]
    public void GapsOfEmptyMapAreWholeOuterRange()
    {
        var map = new RangeInclusiveMap<uint, string>();
        // Should yield the entire outer range.
        AssertYields(map.GetGaps(UIntRange(1, 8)).GetEnumerator(), UIntRange(1, 8));
    }

    [Test]
    public void GapsOfExactlyCoveredRangeAreEmpty()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(1, 6), "a");
        // Should yield no gaps.
        AssertYields(map.GetGaps(UIntRange(1, 6)).GetEnumerator());
    }

    [Test]
    public void GapsIgnoreRangeBeforeOuterRange()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(1, 3), "a");
        // Should yield the entire outer range.
        AssertYields(map.GetGaps(UIntRange(5, 8)).GetEnumerator(), UIntRange(5, 8));
    }

    [Test]
    public void GapsIgnoreRangeEndingJustBeforeOuterRange()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(1, 4), "a");
        // Should yield the entire outer range.
        AssertYields(map.GetGaps(UIntRange(5, 8)).GetEnumerator(), UIntRange(5, 8));
    }

    [Test]
    public void GapsStartAfterRangeOverlappingOuterStart()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(1, 5), "a");
        // Should yield from just past the end of the stored item to the end of the outer range.
        AssertYields(map.GetGaps(UIntRange(5, 8)).GetEnumerator(), UIntRange(6, 8));
    }

    [Test]
    public void GapsStartAfterRangeAtOuterStart()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(5, 6), "a");
        // Should yield from just past the item onwards.
        AssertYields(map.GetGaps(UIntRange(5, 8)).GetEnumerator(), UIntRange(7, 8));
    }

    [Test]
    public void GapsSurroundRangesInsideOuterRange()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(6, 7), "a");
        map.Insert(UIntRange(3, 4), "a");
        // Should yield gaps at start, between items, and at end.
        AssertYields(map.GetGaps(UIntRange(1, 8)).GetEnumerator(), UIntRange(1, 2), UIntRange(5, 5), UIntRange(8, 8));
    }

    [Test]
    public void GapsEndBeforeRangeAtOuterEnd()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(7, 8), "a");
        // Should yield from the start of the outer range up to just before the start of the stored item.
        AssertYields(map.GetGaps(UIntRange(5, 8)).GetEnumerator(), UIntRange(5, 6));
    }

    [Test]
    public void GapsEndBeforeRangeOverlappingOuterEnd()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(5, 6), "a");
        // Should yield from the start of the outer range up to the start of the stored item.
        AssertYields(map.GetGaps(UIntRange(2, 5)).GetEnumerator(), UIntRange(2, 4));
    }

    [Test]
    public void GapsIgnoreRangeStartingJustAfterOuterRange()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(5, 9), "a");
        // Should yield the entire outer range.
        AssertYields(map.GetGaps(UIntRange(1, 4)).GetEnumerator(), UIntRange(1, 4));
    }

    [Test]
    public void GapsIgnoreRangeAfterOuterRange()
    {
        var map = new RangeInclusiveMap<uint, string>();
        map.Insert(UIntRange(6, 7), "a");
        // Should yield the entire outer range.
        AssertYields(map.GetGaps(UIntRange(1, 4)).GetEnumerator(), UIntRange(1, 4));
    }

    [Test]
    public void GapsOfSingleKeyOuterRangeBetweenRanges()
    {
        var map = new RangeInclusiveMap<uint, string>();
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◆---◆ ◌ ◌ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(1, 3), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◌ ◆---◆ ◌ ◌
        map.Insert(UIntRange(5, 7), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◆ ◌ ◌ ◌ ◌ ◌
        // Should yield a zero-width gap.
        AssertYields(map.GetGaps(UIntRange(4, 4)).GetEnumerator(), UIntRange(4, 4));
    }

    [Test]
    public void GapsOfSingleKeyOuterRangeBetweenAdjacentRanges()
    {
        var map = new RangeInclusiveMap<uint, string>();
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◆-◆ ◌ ◌ ◌ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(2, 3), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◌ ◆---◆ ◌ ◌ ◌
        map.Insert(UIntRange(5, 6), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◆ ◌ ◌ ◌ ◌ ◌
        AssertYields(map.GetGaps(UIntRange(4, 4)).GetEnumerator(), UIntRange(4, 4));
    }

    [Test]
    public void GapsOfSingleKeyOuterRangeInsideRangeAreEmpty()
    {
        var map = new RangeInclusiveMap<uint, string>();
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◆-----◆ ◌ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(2, 5), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◆ ◌ ◌ ◌ ◌ ◌
        // Should yield no gaps.
        AssertYields(map.GetGaps(UIntRange(4, 4)).GetEnumerator());
    }

    [Test]
    public void GapsSkipAdjacentRanges()
    {
        // Give the two ranges different values so that they aren't merged.
        var map = new RangeInclusiveMap<uint, bool>();
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◆-◆ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(4, 5), true);
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◆-◆ ◌ ◌ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(2, 3), false);
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ●-------------● ◌
        // Should yield gaps at start and end, but not between the
        // two touching items.
        AssertYields(map.GetGaps(UIntRange(1, 8)).GetEnumerator(), UIntRange(1, 1), UIntRange(6, 8));
    }

    [Test]
    public void GapsDoNotOverflowAtKeyDomainExtremes()
    {
        // Items and outer range both at extremes.
        var map = new RangeInclusiveMap<byte, bool>();
        map.Insert(ByteRange(0, 255), false);
        Assert.That(map.GetGaps(ByteRange(0, 255)), Is.Empty);

        // Items at extremes with gaps in middle.
        map = new RangeInclusiveMap<byte, bool>();
        map.Insert(ByteRange(0, 255), false);
        Assert.That(map.GetGaps(ByteRange(0, 5)), Is.Empty);
        Assert.That(map.GetGaps(ByteRange(250, 255)), Is.Empty);

        // Items just in from extremes.
        map = new RangeInclusiveMap<byte, bool>();
        map.Insert(ByteRange(0, 255), false);
        Assert.That(map.GetGaps(ByteRange(1, 5)), Is.Empty);
        Assert.That(map.GetGaps(ByteRange(250, 254)), Is.Empty);

        // Outer range just in from extremes,
        // items at extremes.
        map = new RangeInclusiveMap<byte, bool>();
        map.Insert(ByteRange(1, 254), false);
        Assert.That(map.GetGaps(ByteRange(0, 5)), Is.EqualTo(new[] { ByteRange(0, 0) }));
        Assert.That(map.GetGaps(ByteRange(250, 255)), Is.EqualTo(new[] { ByteRange(255, 255) }));
    }

    [Test]
    public void GapsBetweenSingleKeyRangesAtKeyDomainExtremes()
    {
        // Two adjacent single-key ranges at each end of the key domain.
        var map = new RangeInclusiveMap<byte, bool>();
        map.Insert(ByteRange(0, 0), false);
        map.Insert(ByteRange(1, 1), true);
        map.Insert(ByteRange(254, 254), false);
        map.Insert(ByteRange(255, 255), true);

        // Should yield one big gap in the middle.
        AssertYields(map.GetGaps(ByteRange(0, 255)).GetEnumerator(), ByteRange(2, 253));
    }

    #endregion

    #region Overlapping tests

    [Test]
    public void GetOverlappingOnEmptyMapIsEmpty()
    {
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◌ ◌ ◌ ◌ ◌ ◌
        var map = new RangeInclusiveMap<uint, string>();
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◆-------------◆ ◌
        // Should not yield any items.
        AssertYields(map.GetOverlapping(UIntRange(1, 8)).GetEnumerator());
    }

    [Test]
    public void GetOverlappingIncludesPartiallyOverlappedRanges()
    {
        var map = new RangeInclusiveMap<uint, string>();

        // 0 1 2 3 4 5 6 7 8 9
        // ●-● ◌ ◌ ◌ ◌ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(0, 1), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ●-● ◌ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(3, 4), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◌ ◌ ●-● ◌ ◌
        map.Insert(UIntRange(6, 7), "a");

        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◆---------◆ ◌ ◌ ◌
        // Should yield partially overlapped range at start,
        // completely overlapped range in middle,
        // and partially overlapped range at end.
        AssertYields(map.GetOverlapping(UIntRange(1, 6)).GetEnumerator(),
            Entry(UIntRange(0, 1), "a"), Entry(UIntRange(3, 4), "a"), Entry(UIntRange(6, 7), "a"));
        AssertYields(map.GetOverlapping(UIntRange(1, 6)).Reverse().GetEnumerator(),
            Entry(UIntRange(6, 7), "a"), Entry(UIntRange(3, 4), "a"), Entry(UIntRange(0, 1), "a"));
    }

    [Test]
    public void GetOverlappingExcludesAdjacentRanges()
    {
        var map = new RangeInclusiveMap<uint, string>();

        // 0 1 2 3 4 5 6 7 8 9
        // ●-● ◌ ◌ ◌ ◌ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(0, 1), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ●-● ◌ ◌ ◌ ◌ ◌
        map.Insert(UIntRange(3, 4), "a");
        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◌ ◌ ◌ ◌ ●-● ◌ ◌
        map.Insert(UIntRange(6, 7), "a");

        // 0 1 2 3 4 5 6 7 8 9
        // ◌ ◌ ◆-----◆ ◌ ◌ ◌ ◌
        // Should yield only the completely overlapped range in the middle,
        // not the adjacent ranges on either side.
        AssertYields(map.GetOverlapping(UIntRange(2, 5)).GetEnumerator(), Entry(UIntRange(3, 4), "a"));
        Assert.That(map.Overlaps(UIntRange(2, 2)), Is.False);
        Assert.That(map.Overlaps(UIntRange(1, 1)), Is.True);
    }

    #endregion

    #region ToString

    [Test]
    public void ToStringListsEntries()
    {
        var map = new RangeInclusiveMap<uint, string>();

        // Empty
        Assert.That(map.ToString(), Is.EqualTo("{}"));

        // One entry
        map.Insert(UIntRange(2, 5), "a");
        Assert.That(map.ToString(), Is.EqualTo("{2..=5: a}"));

        // Many entries
        map.Insert(UIntRange(7, 8), "b");
        map.Insert(UIntRange(10, 11), "c");
        Assert.That(map.ToString(), Is.EqualTo("{2..=5: a, 7..=8: b, 10..=11: c}"));
    }

    #endregion

    #region Equality

    [Test]
    public void MapsWithDifferentRangeEndsAreNotEqual()
    {
        var a = new RangeInclusiveMap<uint, string> { { UIntRange(0, 5), "a" } };
        var b = new RangeInclusiveMap<uint, string> { { UIntRange(0, 2), "a" } };
        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void MapsWithIdenticalRangesAreEqual()
    {
        var a = new RangeInclusiveMap<uint, string> { { UIntRange(0, 5), "a" } };
        var b = new RangeInclusiveMap<uint, string> { { UIntRange(0, 5), "a" } };
        Assert.That(a, Is.EqualTo(b));
        Assert.That(a == b, Is.True);
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        Assert.That(a.CompareTo(b), Is.Zero);
    }

    [Test]
    public void CloneIsIndependent()
    {
        var map = new RangeInclusiveMap<uint, int> { { UIntRange(0, 10), 1 } };
        var clone = map.Clone();
        map.Insert(UIntRange(5, 6), 2);
        Assert.That(clone.ToTuples(), Is.EqualTo(new[] { (UIntRange(0, 10), 1) }));
    }

    #endregion

    #region UInt128 keys

    private static RangeInclusive<UInt128> UInt128Range(UInt128 start, UInt128 end) => new(start, end);

    [Test]
    public void UInt128KeysCoalesceAndSplit()
    {
        var big = UInt128.MaxValue / 2;
        var map = new RangeInclusiveMap<UInt128, string>();
        map.Insert(UInt128Range(big, big + 9u), "a");
        map.Insert(UInt128Range(big + 10u, big + 19u), "a");
        map.Insert(UInt128Range(big + 5u, big + 5u), "b");
        map.ValidateInvariants();
        Assert.That(map.ToTuples(), Is.EqualTo(new[]
        {
            (UInt128Range(big, big + 4u), "a"),
            (UInt128Range(big + 5u, big + 5u), "b"),
            (UInt128Range(big + 6u, big + 19u), "a"),
        }));
        Assert.That(map[big + 19u], Is.EqualTo("a"));
        Assert.That(map.ContainsKey(big + 20u), Is.False);
        Assert.That(
            map.GetGaps(UInt128Range(big - 1u, big + 20u)),
            Is.EqualTo(new[] { UInt128Range(big - 1u, big - 1u), UInt128Range(big + 20u, big + 20u) }));
    }

    [Test]
    public void NoOverflowAtUInt128DomainExtremes()
    {
        var max = UInt128.MaxValue;
        var map = new RangeInclusiveMap<UInt128, bool>();
        map.Insert(UInt128Range(UInt128.Zero, max), false);
        map.Insert(UInt128Range(UInt128.Zero, 10u), true);
        map.Insert(UInt128Range(max - 10u, max), true);
        map.Remove(UInt128Range(UInt128.Zero, 5u));
        map.Remove(UInt128Range(max - 5u, max));
        map.Insert(UInt128Range(max, max), false);
        map.Insert(UInt128Range(UInt128.Zero, UInt128.Zero), false);
        map.ValidateInvariants();
        Assert.That(map.ToTuples(), Is.EqualTo(new[]
        {
            (UInt128Range(UInt128.Zero, UInt128.Zero), false),
            (UInt128Range(6u, 10u), true),
            (UInt128Range(11u, max - 11u), false),
            (UInt128Range(max - 10u, max - 6u), true),
            (UInt128Range(max, max), false),
        }));
        Assert.That(map.GetGaps(UInt128Range(UInt128.Zero, max)), Is.EqualTo(new[] { UInt128Range(1u, 5u), UInt128Range(max - 5u, max - 1u) }));
    }

    #endregion
}
