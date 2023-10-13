// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;
using Google.Protobuf;
using NetTools;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

public class OnionDbGenerator : IDatabaseGenerator
{
    protected readonly OnionooDataSource Onionoo;
    protected readonly LocationDbSource LocationDb;

    public OnionDbGenerator(OnionooDataSource onionoo, LocationDbSource locationDb)
    {
        Onionoo = onionoo;
        LocationDb = locationDb;
    }

    public Task GenerateDatabase(IFileSink fileSink)
    {
        var database = CreateBaseDb();

        // iterate through each country and write info
        foreach (var country in LocationDb.Database.Countries)
        {
            var nodes = Onionoo.Countries.SingleOrDefault(x => x.Key.Equals(country.Code, StringComparison.OrdinalIgnoreCase));

            if (nodes == null)
            {
                continue;
            }

            var countryData = new OnionDbCountry
            {
                CountryCode = country.Code,
                CountryName = country.Name
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

            WriteCountryAddressRanges(countryData, LocationDb.IPv4CountryAddressRanges[country.Code], LocationDb.IPv6CountryAddressRanges[country.Code]);
            database.Countries.Add(countryData);
        }

        OnDatabaseGenerated(fileSink, database);
        return Task.CompletedTask;
    }

    protected virtual OnionDb CreateBaseDb() => new()
    {
        DbFormatVersion = 1,
        MagicBytes = ByteString.CopyFrom(0xDB),
        GeoLicense = LocationDb.Database.License,
        DbVersion = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        TorDirVersion = Onionoo.DataLastModified.ToUnixTimeSeconds(),
        GeoDirVersion = new DateTimeOffset(LocationDb.Database.CreatedAt).ToUnixTimeSeconds()
    };

    protected virtual void WriteCountryAddressRanges(OnionDbCountry country, IEnumerable<IPAddressRange> v4Ranges, IEnumerable<IPAddressRange> v6Ranges)
    {
        country.V4Ranges.AddRange(v4Ranges.Select(x => new IPV4Range
        {
            Start = (uint)IPAddress.HostToNetworkOrder((int)x.Begin.Address),
            End = (uint)IPAddress.HostToNetworkOrder((int)x.End.Address)
        }));

        country.V6Ranges.AddRange(v6Ranges.Select(x => new IPV6Range
        {
            Start = x.Begin.ToString(),
            End = x.End.ToString()
        }));
    }

    protected virtual void OnDatabaseGenerated(IFileSink fileSink, OnionDb database)
    {
        // write onion.db to file
        database.WriteTo(fileSink.CreateFile("onion.db"));
    }
}