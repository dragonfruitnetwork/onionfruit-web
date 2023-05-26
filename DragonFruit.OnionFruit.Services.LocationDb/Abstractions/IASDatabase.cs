// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;

namespace DragonFruit.OnionFruit.Services.LocationDb.Abstractions
{
    public interface IASDatabase : IEnumerable<IDatabaseAS>
    {
        IDatabaseAS this[uint index] { get; }

        IDatabaseAS GetAS(uint number);
    }
}
