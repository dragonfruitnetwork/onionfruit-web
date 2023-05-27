// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.Sync
{
    public interface IDatabaseAccessor
    {
        Task<T> PerformAsync<T>(Func<ILocationDatabase, T> action);
    }
}
