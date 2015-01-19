using System.Collections.Generic;
using System.Linq;

namespace listener.Common.Utils
{
    internal static class ByteArrayExtensions
    {
        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            return string.Join(" ", bytes.Select(b => string.Format("0x{0:X}", b)));
        }

        public static string ToDecString(this IEnumerable<byte> bytes)
        {
            return string.Join(" ", bytes.Select(b => string.Format("{0:G}", b)));
        }
    }
}