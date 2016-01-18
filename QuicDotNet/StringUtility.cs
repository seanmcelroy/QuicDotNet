using System.Linq;
using System.Text;

namespace QuicDotNet
{
    using JetBrains.Annotations;

    public static class StringUtility
    {
        [NotNull, Pure]
        public static string GenerateHexDumpWithASCII([NotNull] this byte[] bytes)
        {
            var i = 0;

            var sb = new StringBuilder();
            while (true)
            {
                var bytesInRow = bytes.Skip(i).Take(16).ToArray();
                if (bytesInRow.Length == 0)
                    break;
                sb.AppendLine($"{i:X4}   {new string(bytesInRow.Select(b => b.ToString("x2")).Aggregate((c, n) => c + " " + n).ToArray())}  {bytesInRow.Select(b => Encoding.ASCII.GetChars(new[] { b })[0].NormalizeHexOutput().ToString()).Aggregate((c, n) => c + n)}");
                i += 16;
            }

            return sb.ToString();
        }

        [Pure]
        private static char NormalizeHexOutput(this char c)
        {
            switch (c)
            {
                case '\0':
                case '\n':
                case '\r':
                case '\t':
                    return ' ';
                default:
                    if (c < 32)
                        return ' ';
                    return c;
            }
        }
    }
}
