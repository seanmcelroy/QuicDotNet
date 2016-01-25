namespace QuicDotNet.Test.Console
{
    using System.Threading.Tasks;

    public class Program
    {
        private static void Main(string[] args)
        {
            var conn = new QuicClient();
            conn.ConnectAsync("www.google.com", 443).RunSynchronously();
        }
    }
}
