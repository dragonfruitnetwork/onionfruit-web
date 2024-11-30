// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using Google.Protobuf;
using NetTools;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

/// <summary>
/// A variant of the onion.db file without address ranges. Used on the DragonFruit website.
/// </summary>
public class OnionLiteDbGenerator(OnionooDataSource onionoo, LocationDbSource locationDb) : OnionDbGenerator(onionoo, locationDb)
{
    protected override OnionDb CreateBaseDb()
    {
        var baseDb = base.CreateBaseDb();

        baseDb.ClearGeoLicense();
        baseDb.ClearGeoDirVersion();

        return baseDb;
    }

    protected override void WriteCountryAddressRanges(OnionDbCountry country, IEnumerable<IPAddressRange> v4Ranges, IEnumerable<IPAddressRange> v6Ranges)
    {
        // litedb doesn't include address ranges
    }

    protected override void OnDatabaseGenerated(FileSink fileSink, OnionDb database)
    {
        database.WriteTo(fileSink.CreateFile("onion.lite.db"));
    }
}