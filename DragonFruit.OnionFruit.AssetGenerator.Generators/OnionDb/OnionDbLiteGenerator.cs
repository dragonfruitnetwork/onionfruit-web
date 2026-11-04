// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.ObjectModel;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core;
using Google.Protobuf;
using RangeMap;
using OnionDbMessage = OnionDb;
using OnionDbCountryMessage = OnionDbCountry;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.OnionDb;

/// <summary>
/// A variant of the onion.db file without address ranges. Used by the DragonFruit website.
/// </summary>
public class OnionDbLiteGenerator : OnionDbGenerator
{
    internal const string LiteDatabaseOutputKey = "onion.lite.db";

    public override string Name => "onion-db-lite";

    public override IReadOnlySet<string> Produces { get; } = new ReadOnlySet<string>(new HashSet<string>
    {
        LiteDatabaseOutputKey
    });

    protected override OnionDbMessage CreateBaseDb()
    {
        var database = base.CreateBaseDb();

        database.ClearGeoLicense();
        database.ClearGeoDirVersion();

        return database;
    }

    protected override OnionDbMessage CreateBaseDb(GenerationContext context)
    {
        var database = base.CreateBaseDb(context);

        // the lite database doesn't contain address ranges, so the geo dataset metadata is omitted too
        database.ClearGeoLicense();
        database.ClearGeoDirVersion();

        return database;
    }

    protected override void WriteCountryAddressRanges(OnionDbCountryMessage country, IEnumerable<RangeInclusive<uint>> v4Ranges, IEnumerable<RangeInclusive<UInt128>> v6Ranges)
    {
        // lite db doesn't include address ranges
    }

    protected override async Task OnDatabaseGeneratedAsync(GenerationContext context, OnionDbMessage database, CancellationToken ct)
    {
        await using var stream = await context.Workspace.CreateFileAsync(LiteDatabaseOutputKey, ct);
        database.WriteTo(stream);
    }
}