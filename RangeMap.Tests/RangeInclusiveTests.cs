// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace RangeMap.Tests;

[TestFixture]
public class RangeInclusiveTests
{
    [Test]
    public void InclusiveRangeIncludesEnd()
    {
        var range = new RangeInclusive<int>(2, 5);
        Assert.That(range.Contains(5), Is.True);
        Assert.That(range.Contains(6), Is.False);
        Assert.That(new RangeInclusive<int>(3, 3).IsEmpty, Is.False);
        Assert.That(new RangeInclusive<int>(4, 3).IsEmpty, Is.True);
        Assert.That(range.ToString(), Is.EqualTo("2..=5"));
    }

    [Test]
    public void ConvertsFromTuplesAndCompareByValue()
    {
        RangeInclusive<int> range = (1, 3);
        Assert.That(range, Is.EqualTo(new RangeInclusive<int>(1, 3)));
        Assert.That(range, Is.Not.EqualTo(new RangeInclusive<int>(1, 4)));
    }
}