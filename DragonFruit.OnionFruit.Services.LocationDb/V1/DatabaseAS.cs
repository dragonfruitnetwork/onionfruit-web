// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Diagnostics;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    [DebuggerDisplay("AS{Number} - {Name}")]
    internal record DatabaseAS(uint Number, string Name) : IDatabaseAS, ISearchableItem<uint>
    {
        uint ISearchableItem<uint>.Key => Number;

        public override string ToString() => $"AS{Number}";
    }
}
