// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Diagnostics;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    [DebuggerDisplay("{Code} - {Name}")]
    internal record DatabaseCountry(string Code, string ContinentCode, string Name) : IDatabaseCountry, ISearchableItem<string>
    {
        string ISearchableItem<string>.Key => Code;
    }
}
