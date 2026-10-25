namespace RangeMap;

internal sealed class RangeTree<TKey, TValue> where TKey : IComparable<TKey>
{
    internal const int MaxLeafCount = 32;
    internal const int MinLeafCount = MaxLeafCount / 2;
    internal const int MaxChildCount = 32;
    internal const int MinChildCount = MaxChildCount / 2;

    internal abstract class Node
    {
        // Number of entries (leaf) or children (branch).
        public int Count;
    }

    internal sealed class Leaf : Node
    {
        // One spare slot so that a full leaf can take an insert before being split.
        public readonly TKey[] Starts = new TKey[MaxLeafCount + 1];
        public readonly TKey[] Ends = new TKey[MaxLeafCount + 1];
        public readonly TValue[] Values = new TValue[MaxLeafCount + 1];
        public Leaf? Prev;
        public Leaf? Next;
    }

    internal sealed class Branch : Node
    {
        public readonly TKey[] Keys = new TKey[MaxChildCount];
        public readonly Node[] Children = new Node[MaxChildCount + 1];
    }

    internal readonly struct Cursor
    {
        public readonly Leaf Leaf;
        public readonly int Index;

        public Cursor(Leaf leaf, int index)
        {
            Leaf = leaf;
            Index = index;
        }

        public TKey Start => Leaf.Starts[Index];
        public TKey End => Leaf.Ends[Index];
        public TValue Value => Leaf.Values[Index];

        public bool SamePositionAs(Cursor other) => Leaf == other.Leaf && Index == other.Index;
    }

    internal struct EntryRange
    {
        private readonly RangeTree<TKey, TValue>? _tree;
        private readonly int _version;
        private Cursor _front;
        private Cursor _back;
        private bool _remaining;
        private Cursor _current;

        public EntryRange(RangeTree<TKey, TValue> tree, Cursor front, Cursor back)
        {
            _tree = tree;
            _version = tree._version;
            _front = front;
            _back = back;
            _remaining = true;
            _current = default;
        }

        public readonly Cursor Current => _current;

        public bool MoveNext()
        {
            if (!_remaining)
            {
                return false;
            }

            _tree!.CheckVersion(_version);
            _current = _front;

            if (_front.SamePositionAs(_back))
            {
                _remaining = false;
            }
            else
            {
                Advance(ref _front);
            }

            return true;
        }

        public bool MoveNextBack()
        {
            if (!_remaining)
            {
                return false;
            }

            _tree!.CheckVersion(_version);
            _current = _back;

            if (_front.SamePositionAs(_back))
            {
                _remaining = false;
            }
            else
            {
                Retreat(ref _back);
            }

            return true;
        }
    }

    private Node? _root;
    private Leaf? _first;
    private Leaf? _last;
    // Number of branch levels above the leaves.
    private int _height;
    private int _count;
    private int _version;

    public int Count => _count;

    public int Version => _version;

    public void CheckVersion(int version)
    {
        if (version != _version)
        {
            throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
        }
    }

    public void Clear()
    {
        _root = null;
        _first = null;
        _last = null;
        _height = 0;
        _count = 0;
        _version++;
    }

    public EntryRange All()
    {
        return TryGetFirst(out var first) && TryGetLast(out var last) ? new EntryRange(this, first, last) : default;
    }

    public bool TryGetFirst(out Cursor cursor)
    {
        if (_count == 0)
        {
            cursor = default;
            return false;
        }

        cursor = new Cursor(_first!, 0);
        return true;
    }

    public bool TryGetLast(out Cursor cursor)
    {
        if (_count == 0)
        {
            cursor = default;
            return false;
        }

        cursor = new Cursor(_last!, _last!.Count - 1);
        return true;
    }

    /// <summary>Moves to the next entry. Returns false (leaving the cursor unusable) at the end.</summary>
    public static bool Advance(ref Cursor cursor)
    {
        if (cursor.Index + 1 < cursor.Leaf.Count)
        {
            cursor = new Cursor(cursor.Leaf, cursor.Index + 1);
            return true;
        }

        var next = cursor.Leaf.Next;
        if (next is null)
        {
            return false;
        }

        cursor = new Cursor(next, 0);
        return true;
    }

    /// <summary>Moves to the previous entry. Returns false (leaving the cursor unusable) at the start.</summary>
    public static bool Retreat(ref Cursor cursor)
    {
        if (cursor.Index > 0)
        {
            cursor = new Cursor(cursor.Leaf, cursor.Index - 1);
            return true;
        }

        var prev = cursor.Leaf.Prev;
        if (prev is null)
        {
            return false;
        }

        cursor = new Cursor(prev, prev.Count - 1);
        return true;
    }

    /// <summary>Finds the last entry whose start is less than or equal to <paramref name="key"/>.</summary>
    public bool TryFindLastAtOrBefore(TKey key, out Cursor cursor)
    {
        if (_root is null)
        {
            cursor = default;
            return false;
        }

        var leaf = FindLeaf(key);
        return TryAt(leaf, UpperBound(leaf.Starts, leaf.Count, key) - 1, out cursor);
    }

    /// <summary>Finds the last entry whose start is less than <paramref name="key"/>.</summary>
    public bool TryFindLastBefore(TKey key, out Cursor cursor)
    {
        if (_root is null)
        {
            cursor = default;
            return false;
        }

        var leaf = FindLeaf(key);
        return TryAt(leaf, LowerBound(leaf.Starts, leaf.Count, key) - 1, out cursor);
    }

    /// <summary>Finds the first entry whose start is greater than <paramref name="key"/>.</summary>
    public bool TryFindFirstAfter(TKey key, out Cursor cursor)
    {
        if (_root is null)
        {
            cursor = default;
            return false;
        }

        var leaf = FindLeaf(key);
        return TryAt(leaf, UpperBound(leaf.Starts, leaf.Count, key), out cursor);
    }

    /// <summary>Finds the first entry whose start is greater than or equal to <paramref name="key"/>.</summary>
    public bool TryFindFirstAtOrAfter(TKey key, out Cursor cursor)
    {
        if (_root is null)
        {
            cursor = default;
            return false;
        }

        var leaf = FindLeaf(key);
        return TryAt(leaf, LowerBound(leaf.Starts, leaf.Count, key), out cursor);
    }

    // Resolves an index that may be one past either end of the leaf.
    private static bool TryAt(Leaf leaf, int index, out Cursor cursor)
    {
        if (index < 0)
        {
            var prev = leaf.Prev;
            cursor = prev is null ? default : new Cursor(prev, prev.Count - 1);
            return prev is not null;
        }

        if (index >= leaf.Count)
        {
            var next = leaf.Next;
            cursor = next is null ? default : new Cursor(next, 0);
            return next is not null;
        }

        cursor = new Cursor(leaf, index);
        return true;
    }

    private Leaf FindLeaf(TKey key)
    {
        var node = _root!;
        for (var height = _height; height > 0; height--)
        {
            var branch = (Branch)node;
            node = branch.Children[UpperBound(branch.Keys, branch.Count - 1, key)];
        }

        return (Leaf)node;
    }

    // Index of the first of keys[0..count) that is greater than key.
    private static int UpperBound(TKey[] keys, int count, TKey key)
    {
        int lo = 0, hi = count;
        while (lo < hi)
        {
            var mid = (lo + hi) >>> 1;
            if (keys[mid].CompareTo(key) <= 0)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    // Index of the first of keys[0..count) that is greater than or equal to key.
    private static int LowerBound(TKey[] keys, int count, TKey key)
    {
        int lo = 0, hi = count;
        while (lo < hi)
        {
            var mid = (lo + hi) >>> 1;
            if (keys[mid].CompareTo(key) < 0)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }

        return lo;
    }

    /// <summary>
    /// Changes the end of the entry at <paramref name="cursor"/>.
    /// Ends don't affect ordering, so this is not a structural modification.
    /// </summary>
    public void SetEnd(Cursor cursor, TKey end)
    {
        cursor.Leaf.Ends[cursor.Index] = end;
        _version++;
    }

    /// <summary>
    /// Inserts an entry, replacing the end and value of any entry with the same start.
    /// Returns true if a new entry was added.
    /// </summary>
    public bool Insert(TKey start, TKey end, TValue value)
    {
        _version++;
        if (_root is null)
        {
            var leaf = new Leaf();
            _root = leaf;
            _first = leaf;
            _last = leaf;
        }

        var added = false;
        var split = InsertInto(_root, _height, start, end, value, ref added, out var splitKey);
        if (split is not null)
        {
            _root = new Branch
            {
                Count = 2,
                Children =
                {
                    [0] = _root,
                    [1] = split,
                },
                Keys =
                {
                    [0] = splitKey,
                },
            };
            _height++;
        }

        if (added)
        {
            _count++;
        }

        return added;
    }

    // Returns the new right sibling if the node had to be split.
    private Node? InsertInto(
        Node node,
        int height,
        TKey start,
        TKey end,
        TValue value,
        ref bool added,
        out TKey splitKey)
    {
        splitKey = default!;
        if (height == 0)
        {
            var leaf = (Leaf)node;
            var i = LowerBound(leaf.Starts, leaf.Count, start);
            if (i < leaf.Count && leaf.Starts[i].CompareTo(start) == 0)
            {
                leaf.Ends[i] = end;
                leaf.Values[i] = value;
                return null;
            }

            InsertAt(leaf.Starts, leaf.Count, i, start);
            InsertAt(leaf.Ends, leaf.Count, i, end);
            InsertAt(leaf.Values, leaf.Count, i, value);

            leaf.Count++;
            added = true;

            if (leaf.Count <= MaxLeafCount)
            {
                return null;
            }

            var right = SplitLeaf(leaf);
            splitKey = right.Starts[0];
            return right;
        }

        var branch = (Branch)node;
        var ci = UpperBound(branch.Keys, branch.Count - 1, start);

        var child = branch.Children[ci];
        var childSplit = InsertInto(child, height - 1, start, end, value, ref added, out var childSplitKey);
        if (childSplit is null)
        {
            return null;
        }

        InsertAt(branch.Keys, branch.Count - 1, ci, childSplitKey);
        InsertAt(branch.Children, branch.Count, ci + 1, childSplit);
        branch.Count++;

        if (branch.Count <= MaxChildCount)
        {
            return null;
        }

        return SplitBranch(branch, out splitKey);
    }

    private Leaf SplitLeaf(Leaf leaf)
    {
        var right = new Leaf();
        var leftCount = leaf.Count / 2;
        var rightCount = leaf.Count - leftCount;

        Array.Copy(leaf.Starts, leftCount, right.Starts, 0, rightCount);
        Array.Copy(leaf.Ends, leftCount, right.Ends, 0, rightCount);
        Array.Copy(leaf.Values, leftCount, right.Values, 0, rightCount);

        Array.Clear(leaf.Starts, leftCount, rightCount);
        Array.Clear(leaf.Ends, leftCount, rightCount);
        Array.Clear(leaf.Values, leftCount, rightCount);

        leaf.Count = leftCount;
        right.Count = rightCount;

        right.Prev = leaf;
        right.Next = leaf.Next;
        if (leaf.Next is null)
        {
            _last = right;
        }
        else
        {
            leaf.Next.Prev = right;
        }

        leaf.Next = right;
        return right;
    }

    private static Branch SplitBranch(Branch branch, out TKey splitKey)
    {
        var right = new Branch();
        var leftCount = branch.Count / 2;
        var rightCount = branch.Count - leftCount;

        // The key between the two halves moves up to the parent.
        splitKey = branch.Keys[leftCount - 1];

        Array.Copy(branch.Children, leftCount, right.Children, 0, rightCount);
        Array.Copy(branch.Keys, leftCount, right.Keys, 0, rightCount - 1);

        Array.Clear(branch.Children, leftCount, rightCount);
        Array.Clear(branch.Keys, leftCount - 1, rightCount);

        branch.Count = leftCount;
        right.Count = rightCount;
        return right;
    }

    /// <summary>Removes the entry with exactly this start, if present.</summary>
    public bool Remove(TKey start)
    {
        if (_root is null || !RemoveFrom(_root, _height, start))
        {
            return false;
        }

        _version++;
        _count--;

        if (_height > 0 && _root.Count == 1)
        {
            _root = ((Branch)_root).Children[0];
            _height--;
        }

        return true;
    }

    private bool RemoveFrom(Node node, int height, TKey start)
    {
        if (height == 0)
        {
            var leaf = (Leaf)node;
            var i = LowerBound(leaf.Starts, leaf.Count, start);
            if (i >= leaf.Count || leaf.Starts[i].CompareTo(start) != 0)
            {
                return false;
            }

            RemoveAt(leaf.Starts, leaf.Count, i);
            RemoveAt(leaf.Ends, leaf.Count, i);
            RemoveAt(leaf.Values, leaf.Count, i);
            leaf.Count--;

            return true;
        }

        var branch = (Branch)node;
        var ci = UpperBound(branch.Keys, branch.Count - 1, start);
        var child = branch.Children[ci];

        if (!RemoveFrom(child, height - 1, start))
        {
            return false;
        }

        if (height == 1)
        {
            if (child.Count < MinLeafCount)
            {
                RebalanceLeaf(branch, ci);
            }
        }
        else if (child.Count < MinChildCount)
        {
            RebalanceBranch(branch, ci);
        }

        return true;
    }

    private void RebalanceLeaf(Branch parent, int index)
    {
        var leaf = (Leaf)parent.Children[index];

        if (index > 0)
        {
            var left = (Leaf)parent.Children[index - 1];
            if (left.Count > MinLeafCount)
            {
                // Borrow the last entry of the left sibling.
                var last = left.Count - 1;
                InsertAt(leaf.Starts, leaf.Count, 0, left.Starts[last]);
                InsertAt(leaf.Ends, leaf.Count, 0, left.Ends[last]);
                InsertAt(leaf.Values, leaf.Count, 0, left.Values[last]);
                leaf.Count++;

                RemoveAt(left.Starts, left.Count, last);
                RemoveAt(left.Ends, left.Count, last);
                RemoveAt(left.Values, left.Count, last);
                left.Count--;

                parent.Keys[index - 1] = leaf.Starts[0];
                return;
            }
        }

        if (index < parent.Count - 1)
        {
            var right = (Leaf)parent.Children[index + 1];
            if (right.Count > MinLeafCount)
            {
                // Borrow the first entry of the right sibling.
                leaf.Starts[leaf.Count] = right.Starts[0];
                leaf.Ends[leaf.Count] = right.Ends[0];
                leaf.Values[leaf.Count] = right.Values[0];
                leaf.Count++;

                RemoveAt(right.Starts, right.Count, 0);
                RemoveAt(right.Ends, right.Count, 0);
                RemoveAt(right.Values, right.Count, 0);
                right.Count--;

                parent.Keys[index] = right.Starts[0];
                return;
            }
        }

        MergeLeaves(parent, index > 0 ? index - 1 : index);
    }

    // Merges Children[leftIndex + 1] into Children[leftIndex].
    private void MergeLeaves(Branch parent, int leftIndex)
    {
        var left = (Leaf)parent.Children[leftIndex];
        var right = (Leaf)parent.Children[leftIndex + 1];

        Array.Copy(right.Starts, 0, left.Starts, left.Count, right.Count);
        Array.Copy(right.Ends, 0, left.Ends, left.Count, right.Count);
        Array.Copy(right.Values, 0, left.Values, left.Count, right.Count);
        left.Count += right.Count;

        left.Next = right.Next;
        if (right.Next is null)
        {
            _last = left;
        }
        else
        {
            right.Next.Prev = left;
        }

        RemoveAt(parent.Keys, parent.Count - 1, leftIndex);
        RemoveAt(parent.Children, parent.Count, leftIndex + 1);
        parent.Count--;
    }

    private static void RebalanceBranch(Branch parent, int index)
    {
        var node = (Branch)parent.Children[index];

        if (index > 0)
        {
            var left = (Branch)parent.Children[index - 1];
            if (left.Count > MinChildCount)
            {
                // Rotate the last child of the left sibling through the parent.
                InsertAt(node.Keys, node.Count - 1, 0, parent.Keys[index - 1]);
                InsertAt(node.Children, node.Count, 0, left.Children[left.Count - 1]);
                node.Count++;

                parent.Keys[index - 1] = left.Keys[left.Count - 2];

                left.Keys[left.Count - 2] = default!;
                left.Children[left.Count - 1] = null!;
                left.Count--;
                return;
            }
        }

        if (index < parent.Count - 1)
        {
            var right = (Branch)parent.Children[index + 1];
            if (right.Count > MinChildCount)
            {
                // Rotate the first child of the right sibling through the parent.
                node.Keys[node.Count - 1] = parent.Keys[index];
                node.Children[node.Count] = right.Children[0];
                node.Count++;

                parent.Keys[index] = right.Keys[0];

                RemoveAt(right.Keys, right.Count - 1, 0);
                RemoveAt(right.Children, right.Count, 0);
                right.Count--;
                return;
            }
        }

        MergeBranches(parent, index > 0 ? index - 1 : index);
    }

    // Merges Children[leftIndex + 1] into Children[leftIndex], pulling their separator down.
    private static void MergeBranches(Branch parent, int leftIndex)
    {
        var left = (Branch)parent.Children[leftIndex];
        var right = (Branch)parent.Children[leftIndex + 1];

        left.Keys[left.Count - 1] = parent.Keys[leftIndex];
        Array.Copy(right.Keys, 0, left.Keys, left.Count, right.Count - 1);
        Array.Copy(right.Children, 0, left.Children, left.Count, right.Count);
        left.Count += right.Count;

        RemoveAt(parent.Keys, parent.Count - 1, leftIndex);
        RemoveAt(parent.Children, parent.Count, leftIndex + 1);
        parent.Count--;
    }

    private static void InsertAt<T>(T[] array, int count, int index, T item)
    {
        if (index < count)
        {
            Array.Copy(array, index, array, index + 1, count - index);
        }

        array[index] = item;
    }

    private static void RemoveAt<T>(T[] array, int count, int index)
    {
        if (index < count - 1)
        {
            Array.Copy(array, index + 1, array, index, count - index - 1);
        }

        array[count - 1] = default!;
    }

    public RangeTree<TKey, TValue> Clone()
    {
        var clone = new RangeTree<TKey, TValue>();
        if (_root is null)
        {
            return clone;
        }

        Leaf? previousLeaf = null;
        clone._root = CloneNode(_root, _height, clone, ref previousLeaf);
        clone._last = previousLeaf;
        clone._height = _height;
        clone._count = _count;
        return clone;
    }

    private static Node CloneNode(Node node, int height, RangeTree<TKey, TValue> clone, ref Leaf? previousLeaf)
    {
        if (height == 0)
        {
            var leaf = (Leaf)node;
            var copy = new Leaf { Count = leaf.Count, Prev = previousLeaf };

            Array.Copy(leaf.Starts, copy.Starts, leaf.Count);
            Array.Copy(leaf.Ends, copy.Ends, leaf.Count);
            Array.Copy(leaf.Values, copy.Values, leaf.Count);

            if (previousLeaf is null)
            {
                clone._first = copy;
            }
            else
            {
                previousLeaf.Next = copy;
            }

            previousLeaf = copy;
            return copy;
        }

        var branch = (Branch)node;
        var branchCopy = new Branch { Count = branch.Count };
        Array.Copy(branch.Keys, branchCopy.Keys, branch.Count - 1);

        for (var i = 0; i < branch.Count; i++)
        {
            branchCopy.Children[i] = CloneNode(branch.Children[i], height - 1, clone, ref previousLeaf);
        }

        return branchCopy;
    }

    internal void Validate()
    {
        if (_root is null)
        {
            Check(_count == 0 && _height == 0 && _first is null && _last is null, "empty tree has stale state");
            return;
        }

        var leaves = new List<Leaf>();
        var count = ValidateNode(_root, _height, default!, false, default!, false, true, leaves);

        Check(count == _count, $"count is {_count} but tree holds {count} entries");
        Check(_first == leaves[0], "first leaf pointer is wrong");
        Check(_last == leaves[^1], "last leaf pointer is wrong");

        for (var i = 0; i < leaves.Count; i++)
        {
            Check(leaves[i].Prev == (i > 0 ? leaves[i - 1] : null), $"leaf {i} has wrong Prev link");
            Check(leaves[i].Next == (i < leaves.Count - 1 ? leaves[i + 1] : null), $"leaf {i} has wrong Next link");
        }
    }

    private static int ValidateNode(
        Node node,
        int height,
        TKey lower,
        bool hasLower,
        TKey upper,
        bool hasUpper,
        bool isRoot,
        List<Leaf> leaves)
    {
        if (height == 0)
        {
            Check(node is Leaf, "expected a leaf at height 0");
            var leaf = (Leaf)node;

            Check(leaf.Count <= MaxLeafCount, "leaf is overfull");
            Check(isRoot || leaf.Count >= MinLeafCount, "leaf is underfull");

            for (var i = 0; i < leaf.Count; i++)
            {
                var increasing = i == 0 || leaf.Starts[i - 1].CompareTo(leaf.Starts[i]) < 0;
                Check(increasing, "leaf starts are not strictly increasing");
                Check(!hasLower || leaf.Starts[i].CompareTo(lower) >= 0, "entry is below its subtree's lower bound");
                Check(!hasUpper || leaf.Starts[i].CompareTo(upper) < 0, "entry is not below its subtree's upper bound");
            }

            leaves.Add(leaf);
            return leaf.Count;
        }

        Check(node is Branch, "expected a branch above height 0");
        var branch = (Branch)node;

        Check(branch.Count <= MaxChildCount, "branch is overfull");
        Check(isRoot ? branch.Count >= 2 : branch.Count >= MinChildCount, "branch is underfull");

        for (var i = 0; i < branch.Count - 1; i++)
        {
            var increasing = i == 0 || branch.Keys[i - 1].CompareTo(branch.Keys[i]) < 0;
            Check(increasing, "branch keys are not strictly increasing");
            Check(!hasLower || branch.Keys[i].CompareTo(lower) >= 0, "separator is below its subtree's lower bound");
            Check(!hasUpper || branch.Keys[i].CompareTo(upper) < 0, "separator is not below its subtree's upper bound");
        }

        var total = 0;
        for (var i = 0; i < branch.Count; i++)
        {
            var childHasLower = i > 0 || hasLower;
            var childLower = i > 0 ? branch.Keys[i - 1] : lower;
            var childHasUpper = i < branch.Count - 1 || hasUpper;
            var childUpper = i < branch.Count - 1 ? branch.Keys[i] : upper;

            var child = branch.Children[i];
            total += ValidateNode(
                child, height - 1, childLower, childHasLower, childUpper, childHasUpper, isRoot: false, leaves);
        }

        return total;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException("RangeTree invariant violated: " + message);
        }
    }
}
