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
            var info = await Task.Run(() => Source.GetSource());

            Console.WriteLine("{0,-20} {1,10} {2,-30}\n", "Country", "Bandwidth", "Flags");
            foreach (var relay in info.Relays.OrderBy(x => x.Bandwidth))
                Console.WriteLine($"{relay.CountryName,-20} {new Bandwidth(relay.Bandwidth).Megabits,10} {relay.Flags.ToString(),-30}");
        }
    }
}
