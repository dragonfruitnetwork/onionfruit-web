// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.OnionFruit.Services.LocationDb.Abstractions
{
    public interface IBinarySearchable<T> where T : ISearchableItem
    {
        int Count { get; }

        ISearchableItem this[int index] { get; }
    }
}
