namespace QuicDotNet.Test.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = new QuicClient();
            conn.Connect("www.google.com", 443);
        }
    }
}
