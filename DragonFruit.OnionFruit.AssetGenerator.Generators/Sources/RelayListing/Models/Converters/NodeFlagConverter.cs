// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Web.Shared.Redis.Nodes;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing.Models.Converters;

internal static class NodeFlagConverter
{
    public static TorNodeFlags Parse(IEnumerable<string> flags)
    {
        var data = flags.Aggregate(TorNodeFlags.None, (a, b) =>
        {
            if (Enum.TryParse(b, out TorNodeFlags flag))
            {
                return a | flag;
            }

            return flag;
        });

        // if there are flags, untoggle the none flag
        return data > TorNodeFlags.None ? data & ~TorNodeFlags.None : data;
    }

    public static string[] ToString(TorNodeFlags flags)
    {
        return Enum.GetValues<TorNodeFlags>()
            .Where(x => (flags & x) == x)
            .Select(x => x.ToString())
            .ToArray();
    }
}