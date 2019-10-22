using System;
using System.Linq;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Status.Converters;

namespace DragonFruit.OnionFruit.Status.Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var Info = await Source.GetSource();

            Console.WriteLine("{0,-20} {1,10} {2,-30}\n", "Country", "Bandwidth", "Flags");
            foreach (var Relay in Info.Relays.OrderBy(x => x.Bandwidth))
                Console.WriteLine($"{Relay.CountryName,-20} {new Bandwidth(Relay.Bandwidth).Megabits,10} {Relay.Flags.ToString(),-30}");
        }
    }
}
