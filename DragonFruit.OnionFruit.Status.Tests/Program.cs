// OnionFruit.Status Copyright 2021 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;
using System.Linq;
using DragonFruit.Common.Data;
using DragonFruit.OnionFruit.Status.Helpers;

namespace DragonFruit.OnionFruit.Status.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = new ApiClient
            {
                UserAgent = "OnionFruit"
            };

            var info = client.GetServerInfo();

            Console.WriteLine($"{"Country",-20} {"Bandwidth",10} {"Flags",-30}\n");

            foreach (var relay in info.Relays.OrderBy(x => x.Bandwidth))
            {
                Console.WriteLine($"{relay.CountryCode,-20} {new Bandwidth(relay.Bandwidth).Megabits,10:f2} {relay.Flags.ToString(),-30}");
            }
        }
    }
}
