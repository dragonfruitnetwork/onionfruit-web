using System.Threading.Tasks;
using DragonFruit.OnionFruit.Api.Status.Enums;
using DragonFruit.OnionFruit.Api.Status.Extensions;
using NUnit.Framework;

namespace DragonFruit.OnionFruit.Api.Tests
{
    [TestFixture]
    public class StatusTests : OnionFruitApiTest
    {
        [Test]
        public async Task TestSummary()
        {
            var summary = await Client.GetTorSummaryAsync(500);
            Assert.AreEqual(500, summary.Relays.Length);
        }

        [Test]
        public async Task TestBridgeSummary()
        {
            var summary = await Client.GetTorSummaryAsync(200, type: TorNodeType.Bridge);
            Assert.AreEqual(200, summary.Bridges.Length);
        }
        
        [Test]
        public async Task TestDetails()
        {
            var details = await Client.GetTorDetailsAsync(250);
            Assert.AreEqual(250, details.Relays.Length);
        }

        [Test]
        public async Task TestBandwidthHistory()
        {
            var history = await Client.GetBandwidthHistoryAsync(750);
            Assert.AreEqual(history.Relays.Length, 750);
        }
    }
}