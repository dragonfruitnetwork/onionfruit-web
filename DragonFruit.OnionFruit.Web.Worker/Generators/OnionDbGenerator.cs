using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using Google.Protobuf;
using NetTools;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

public class OnionDbGenerator(OnionooDataSource onionoo, LocationDbSource locationDb) : IDatabaseGenerator
{
    public Task GenerateDatabase(FileSink fileSink)
    {
        var database = CreateBaseDb();

        // iterate through each country and write info
        foreach (var country in locationDb.Database.Countries)
        {
            var nodes = onionoo.Countries.SingleOrDefault(x => x.Key.Equals(country.Code, StringComparison.OrdinalIgnoreCase));

            if (nodes == null)
            {
                continue;
            }

            var countryData = new OnionDbCountry
            {
                CountryCode = country.Code,
                CountryName = CountryMap.Instance.GetCountryName(country.Code) ?? country.Name
            };

            // calculate total node counts
            foreach (var nodeFlag in nodes.SelectMany(x => x.RawFlags))
            {
                switch (nodeFlag)
                {
                    case nameof(TorNodeFlags.Exit):
                        countryData.ExitNodeCount++;
                        break;

                    case nameof(TorNodeFlags.Guard):
                        countryData.EntryNodeCount++;
                        break;

                    case nameof(TorNodeFlags.Fast):
                        countryData.FastNodeCount++;
                        break;

                    case nameof(TorNodeFlags.Running):
                        countryData.OnlineNodeCount++;
                        break;
                }

                countryData.TotalNodeCount++;
            }

            WriteCountryAddressRanges(countryData, locationDb.IPv4CountryAddressRanges[country.Code], locationDb.IPv6CountryAddressRanges[country.Code]);
            database.Countries.Add(countryData);
        }

        OnDatabaseGenerated(fileSink, database);
        return Task.CompletedTask;
    }

    protected virtual OnionDb CreateBaseDb() => new()
    {
        MagicBytes = ByteString.CopyFrom(0xDB, 0x01),

        // tor license can be found at the footer of https://metrics.torproject.org/onionoo.html
        TorLicense = "CC0",
        GeoLicense = locationDb.Database.License,

        // versions derived from datasets/current epoch
        DbVersion = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        TorDirVersion = onionoo.DataLastModified.ToUnixTimeSeconds(),
        GeoDirVersion = locationDb.Database.CreatedAt.ToUnixTimeSeconds()
    };

    protected virtual void WriteCountryAddressRanges(OnionDbCountry country, IEnumerable<IPAddressRange> v4Ranges, IEnumerable<IPAddressRange> v6Ranges)
    {
        country.V4Ranges.AddRange(v4Ranges.Select(x => new AddressRange
        {
            Start = ByteString.CopyFrom(x.Begin.GetAddressBytes()),
            End = ByteString.CopyFrom(x.End.GetAddressBytes())
        }));

        country.V6Ranges.AddRange(v6Ranges.Select(x => new AddressRange
        {
            Start = ByteString.CopyFrom(x.Begin.GetAddressBytes()),
            End = ByteString.CopyFrom(x.End.GetAddressBytes())
        }));
    }

    protected virtual void OnDatabaseGenerated(FileSink fileSink, OnionDb database)
    {
        // write onion.db to file
        var dbStream = fileSink.CreateFile("onion.db");

        database.WriteTo(dbStream);

        // write brotli compressed version as well
        using var compressionStream = new BrotliStream(fileSink.CreateFile("onion.db.br"), CompressionLevel.SmallestSize, true);

        dbStream.Seek(0, SeekOrigin.Begin);
        dbStream.CopyTo(compressionStream);
    }
}