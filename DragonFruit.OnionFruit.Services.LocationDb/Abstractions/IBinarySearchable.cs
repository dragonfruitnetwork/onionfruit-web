// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.OnionFruit.Services.LocationDb.Abstractions
{
    public interface IBinarySearchable<out T, TKey> where T : ISearchableItem<TKey>
    {
        int Count { get; }

        T this[int index] { get; }
    }
}
