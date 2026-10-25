// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace RangeMap.Tests;

[TestFixture]
public class RangeTreeTests
{
    private sealed class Model : SortedDictionary<int, (int End, int Value)>;

    private static List<(int Start, int End, int Value)> Forward(RangeTree<int, int> tree)
    {
        var list = new List<(int, int, int)>();
        var entries = tree.All();
        while (entries.MoveNext())
        {
            var entry = entries.Current;
            list.Add((entry.Start, entry.End, entry.Value));
        }

        return list;
    }

    private static List<(int Start, int End, int Value)> Backward(RangeTree<int, int> tree)
    {
        var list = new List<(int, int, int)>();
        var entries = tree.All();
        while (entries.MoveNextBack())
        {
            var entry = entries.Current;
            list.Add((entry.Start, entry.End, entry.Value));
        }

        return list;
    }

    private static void AssertMatches(RangeTree<int, int> tree, Model model)
    {
        tree.Validate();
        Assert.That(tree.Count, Is.EqualTo(model.Count));
        var expected = model.Select(kv => (kv.Key, kv.Value.End, kv.Value.Value)).ToList();
        Assert.That(Forward(tree), Is.EqualTo(expected));
        expected.Reverse();
        Assert.That(Backward(tree), Is.EqualTo(expected));
    }

    private static void AssertFindsMatch(RangeTree<int, int> tree, int[] keys, int x)
    {
        var i = Array.BinarySearch(keys, x);
        var lower = i >= 0 ? i : ~i; // first key >= x
        var upper = i >= 0 ? i + 1 : ~i; // first key > x

        AssertFound(tree.TryFindLastAtOrBefore(x, out var cursor), cursor, upper > 0 ? keys[upper - 1] : null);
        AssertFound(tree.TryFindLastBefore(x, out cursor), cursor, lower > 0 ? keys[lower - 1] : null);
        AssertFound(tree.TryFindFirstAfter(x, out cursor), cursor, upper < keys.Length ? keys[upper] : null);
        AssertFound(tree.TryFindFirstAtOrAfter(x, out cursor), cursor, lower < keys.Length ? keys[lower] : null);
    }

    private static void AssertFound(bool found, RangeTree<int, int>.Cursor cursor, int? expectedStart)
    {
        Assert.That(found ? cursor.Start : null, Is.EqualTo(expectedStart));
    }

    [TestCase(1, 50, 4_000)]
    [TestCase(2, 1_000, 40_000)]
    [TestCase(3, 1_000_000, 100_000)]
    public void RandomOperationsMatchSortedDictionary(int seed, int domain, int operations)
    {
        var random = new Random(seed);
        var tree = new RangeTree<int, int>();
        var model = new Model();

        // Grow the tree for the first half, then shrink it, so that
        // splits and merges both happen at every level.
        for (var op = 0; op < operations; op++)
        {
            var growing = op < operations / 2;
            var key = random.Next(domain);
            if (random.Next(3) == (growing ? 0 : 1))
            {
                Assert.That(tree.Remove(key), Is.EqualTo(model.Remove(key)));
            }
            else
            {
                var entry = (End: key + random.Next(1, 10), Value: random.Next(4));
                Assert.That(tree.Insert(key, entry.End, entry.Value), Is.EqualTo(model.TryAdd(key, entry)));
                model[key] = entry;
            }

            if (op % 997 == 0)
            {
                tree.Validate();
            }

            if (op == operations / 2 || op == operations - 1)
            {
                AssertMatches(tree, model);
            }
        }

        var keys = model.Keys.ToArray();
        for (var probe = 0; probe < 2_000; probe++)
        {
            AssertFindsMatch(tree, keys, random.Next(-1, domain + 1));
        }
    }

    [TestCase(20_000)]
    public void SequentialInsertsThenRemovalsInEveryOrder(int n)
    {
        IEnumerable<int> Ascending() => Enumerable.Range(0, n);
        IEnumerable<int> Descending() => Enumerable.Range(0, n).Reverse();
        IEnumerable<int> EvensThenOdds() => Ascending().Where(i => i % 2 == 0).Concat(Ascending().Where(i => i % 2 == 1));
        IEnumerable<int> MiddleOut() => Ascending().OrderBy(i => Math.Abs(i - n / 2));

        foreach (var insertOrder in new[] {Ascending(), Descending()})
        {
            foreach (var removeOrder in new[] {Ascending(), Descending(), EvensThenOdds(), MiddleOut()})
            {
                var tree = new RangeTree<int, int>();
                var model = new Model();

                foreach (var key in insertOrder)
                {
                    tree.Insert(key, key + 1, key);
                    model[key] = (key + 1, key);
                }

                AssertMatches(tree, model);

                var removed = 0;
                foreach (var key in removeOrder)
                {
                    Assert.That(tree.Remove(key), Is.True);
                    model.Remove(key);
                    if (++removed % 1_009 == 0)
                    {
                        AssertMatches(tree, model);
                    }
                }

                AssertMatches(tree, model);
                Assert.That(tree.Count, Is.Zero);
            }
        }
    }

    [Test]
    public void EmptyTreeFindsNothing()
    {
        var tree = new RangeTree<int, int>();
        AssertFindsMatch(tree, [], 0);
        Assert.That(tree.TryGetFirst(out _), Is.False);
        Assert.That(tree.TryGetLast(out _), Is.False);
        Assert.That(tree.Remove(0), Is.False);
        Assert.That(Forward(tree), Is.Empty);

        // Emptied by removal rather than never filled.
        tree.Insert(1, 2, 3);
        tree.Remove(1);
        AssertMatches(tree, new Model());
        AssertFindsMatch(tree, [], 1);
    }

    [Test]
    public void InsertWithExistingStartReplacesEndAndValue()
    {
        var tree = new RangeTree<int, int>();
        Assert.That(tree.Insert(5, 10, 1), Is.True);
        Assert.That(tree.Insert(5, 7, 2), Is.False);
        Assert.That(Forward(tree), Is.EqualTo(new[] {(5, 7, 2)}));
    }

    [Test]
    public void SetEndChangesOnlyTheEnd()
    {
        var tree = new RangeTree<int, int>();
        for (var i = 0; i < 100; i++)
        {
            tree.Insert(i * 10, i * 10 + 5, i);
        }

        Assert.That(tree.TryFindLastAtOrBefore(503, out var cursor), Is.True);
        tree.SetEnd(cursor, 509);
        Assert.That(tree.TryFindFirstAtOrAfter(500, out cursor), Is.True);
        Assert.That((cursor.Start, cursor.End, cursor.Value), Is.EqualTo((500, 509, 50)));
        tree.Validate();
    }

    [Test]
    public void CloneIsIndependent()
    {
        var tree = new RangeTree<int, int>();
        var model = new Model();
        for (var i = 0; i < 5_000; i++)
        {
            tree.Insert(i, i + 1, i);
            model[i] = (i + 1, i);
        }

        var clone = tree.Clone();
        AssertMatches(clone, model);

        for (var i = 0; i < 5_000; i += 2)
        {
            tree.Remove(i);
        }

        tree.Insert(-1, 0, 0);
        AssertMatches(clone, model);

        clone.Clear();
        AssertMatches(clone, new Model());
        Assert.That(tree.Count, Is.EqualTo(2_501));
        Assert.That(new RangeTree<int, int>().Clone().Count, Is.Zero);
    }

    [Test]
    public void EntryRangeIsDoubleEnded()
    {
        var tree = new RangeTree<int, int>();
        for (var i = 0; i < 200; i++)
        {
            tree.Insert(i, i + 1, i);
        }

        Assert.That(tree.TryFindFirstAtOrAfter(10, out var front), Is.True);
        Assert.That(tree.TryFindLastAtOrBefore(150, out var back), Is.True);

        // Alternate ends; the two ends should meet without skipping or repeating anything.
        var entries = new RangeTree<int, int>.EntryRange(tree, front, back);
        var seen = new List<int>();
        for (var i = 0;; i++)
        {
            if (!(i % 3 == 0 ? entries.MoveNextBack() : entries.MoveNext()))
            {
                break;
            }

            seen.Add(entries.Current.Start);
        }

        Assert.That(seen.Order(), Is.EqualTo(Enumerable.Range(10, 141)));
        Assert.That(entries.MoveNext(), Is.False);
        Assert.That(entries.MoveNextBack(), Is.False);

        var single = new RangeTree<int, int>.EntryRange(tree, front, front);
        Assert.That(single.MoveNextBack(), Is.True);
        Assert.That(single.Current.Start, Is.EqualTo(10));
        Assert.That(single.MoveNext(), Is.False);

        Assert.That(default(RangeTree<int, int>.EntryRange).MoveNext(), Is.False);
    }

    [Test]
    public void ModificationDuringEnumerationThrows()
    {
        var tree = new RangeTree<int, int>();
        for (var i = 0; i < 10; i++)
        {
            tree.Insert(i, i + 1, i);
        }

        var entries = tree.All();
        Assert.That(entries.MoveNext(), Is.True);
        tree.Remove(5);
        Assert.Throws<InvalidOperationException>(() => entries.MoveNext());
    }

    [Test]
    public void WorksWithReferenceTypeKeysAndValues()
    {
        var tree = new RangeTree<string, string>();
        var words = Enumerable.Range(0, 1_000).Select(i => i.ToString("D4")).ToArray();

        foreach (var w in words.Reverse())
        {
            tree.Insert(w, w + "~", w.ToUpperInvariant());
        }

        foreach (var w in words.Where((_, i) => i % 3 == 0))
        {
            tree.Remove(w);
        }

        tree.Validate();
        Assert.That(tree.Count, Is.EqualTo(666));
        Assert.That(tree.TryFindFirstAtOrAfter("0000", out var cursor) && cursor.Start == "0001", Is.True);
    }
}