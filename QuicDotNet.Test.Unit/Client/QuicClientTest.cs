namespace QuicDotNet.Test.Unit.Client
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class QuicClientTest
    {
        [TestMethod]
        public async Task SendFreshInchoateClientHello()
        {
            using (var client = new QuicClient())
            {
                await client.ConnectAsync("clients2.google.com", 443);

                Thread.Sleep(1000);
            }
        }
    }
}
