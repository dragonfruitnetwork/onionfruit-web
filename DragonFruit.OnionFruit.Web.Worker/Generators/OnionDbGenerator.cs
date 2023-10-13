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
    private readonly OnionooDataSource _onionoo;
    private readonly LocationDbSource _locationDb;

    public OnionDbGenerator(OnionooDataSource onionoo, LocationDbSource locationDb)
    {
        _onionoo = onionoo;
        _locationDb = locationDb;
    }

    public Task GenerateDatabase(IFileSink fileSink)
    {
        var database = CreateBaseDb();

        // iterate through each country and write info
        foreach (var country in _locationDb.Database.Countries)
        {
            var nodes = _onionoo.Countries.SingleOrDefault(x => x.Key.Equals(country.Code, StringComparison.OrdinalIgnoreCase));

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

            WriteCountryAddressRanges(countryData, _locationDb.IPv4CountryAddressRanges[country.Code], _locationDb.IPv6CountryAddressRanges[country.Code]);
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
        GeoLicense = _locationDb.Database.License,

        // versions derived from datasets/current epoch
        DbVersion = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        TorDirVersion = _onionoo.DataLastModified.ToUnixTimeSeconds(),
        GeoDirVersion = _locationDb.Database.CreatedAt.ToUnixTimeSeconds()
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