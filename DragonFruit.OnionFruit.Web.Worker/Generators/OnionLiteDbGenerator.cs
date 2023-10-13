using System.Collections.Generic;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;
using Google.Protobuf;
using NetTools;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

/// <summary>
/// A variant of the onion.db file without address ranges. Used on the DragonFruit website.
/// </summary>
public class OnionLiteDbGenerator : OnionDbGenerator
{
    public OnionLiteDbGenerator(OnionooDataSource onionoo, LocationDbSource locationDb)
        : base(onionoo, locationDb)
    {
    }
    
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

    protected override void OnDatabaseGenerated(IFileSink fileSink, OnionDb database)
    {
        database.WriteTo(fileSink.CreateFile("onion.lite.db"));
    }
}