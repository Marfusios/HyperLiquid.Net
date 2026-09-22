using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Utils;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace HyperLiquid.Net.UnitTests
{
    [TestFixture]
    public class HyperLiquidOutcomeSymbolTests
    {
        [TestCase("#1230", 100001230)]
        [TestCase("#1231", 100001231)]
        [TestCase("#49320", 100049320)]
        [TestCase("#49321", 100049321)]
        [TestCase("#2047483640", 2147483640)]
        [TestCase("#2047483641", 2147483641)]
        public async Task GetSymbolIdFromNameAsync_ShouldMapOutcomeCoinWithoutMetadataRequest(string coin, int expected)
        {
            var client = new Mock<IHyperLiquidRestClient>(MockBehavior.Strict);

            var result = await HyperLiquidUtils.GetSymbolIdFromNameAsync(client.Object, coin);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.EqualTo(expected));
            client.VerifyNoOtherCalls();
        }

        [TestCase("#")]
        [TestCase("#1232")]
        [TestCase("#1239")]
        [TestCase("#-10")]
        [TestCase("#+10")]
        [TestCase("# 10")]
        [TestCase("#10 ")]
        [TestCase("#0010")]
        [TestCase("#1230.0")]
        [TestCase("#2047483650")]
        [TestCase("#2147483640")]
        [TestCase("#99999999999999999999")]
        public async Task GetSymbolIdFromNameAsync_ShouldRejectInvalidOutcomeEncodingWithoutMetadataRequest(string coin)
        {
            var client = new Mock<IHyperLiquidRestClient>(MockBehavior.Strict);

            var result = await HyperLiquidUtils.GetSymbolIdFromNameAsync(client.Object, coin);

            Assert.That(result.Success, Is.False);
            client.VerifyNoOtherCalls();
        }
    }
}
