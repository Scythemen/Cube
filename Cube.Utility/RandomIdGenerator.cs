using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Cube.Utility
{
    //https://stackoverflow.com/questions/9543715/generating-human-readable-usable-short-but-unique-ids

    //public void TestRandomIdGenerator()
    //{
    //    // create five IDs of six, base 62 characters
    //    for (int i = 0; i < 5; i++) Console.WriteLine(RandomIdGenerator.GetBase62(6));

    //    // create five IDs of eight base 36 characters
    //    for (int i = 0; i < 5; i++) Console.WriteLine(RandomIdGenerator.GetBase36(8));
    //}


    public static class RandomIdGenerator
    {
        private static readonly char[] _base62chars =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
            .ToCharArray();

        // remove the ambiguous characters, for human readable
        private static char[] _base32 = "ZYXWVUTSRQPONMLKJIHGFEDCBA765432".ToCharArray();
        //ZYXWVUTSRQPONMLKJIHGFEDCBA765432
        //234567ABCDEFGHIJKLMNOPQRSTUVWXYZ
        public static string GetBase62(int length)
        {
            Random _random = new Random(DateTime.Now.Millisecond + Environment.CurrentManagedThreadId);
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(_base62chars[_random.Next(i, 62)]);
            }

            return sb.ToString();
        }

        public static string GetBase36(int length)
        {
            Random _random = new Random(DateTime.Now.Millisecond + Environment.CurrentManagedThreadId);
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(_base62chars[_random.Next(i, 36)]);
            }

            return sb.ToString();
        }

        private static RandomNumberGenerator RandomNumberGenerator = RandomNumberGenerator.Create();

        // human readable
        public static string GetBase32(int length)
        {
            length = length < 1 ? 4 : length;

            var blen = length * 5;
            blen = (blen >> 2) + (((blen & 3) > 0) ? 1 : 0);
            var randomBytes = new byte[blen];
            RandomNumberGenerator.GetBytes(randomBytes);

            var sb = new StringBuilder(blen * 8);
            foreach (var item in randomBytes)
            {
                sb.Append(Convert.ToString(item, 2).PadLeft(8, '0'));
            }

            var ch = sb.ToString().ToCharArray();
            var res = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                var s = new string(ch.Skip(ch.Length - i * 5 - 5).Take(5).ToArray());
                var index = Convert.ToInt16(s, 2);
                res.Append(_base32[index]);
            }

            return res.ToString();
        }

        public static string ToBase32(long b)
        {
            var sb = new StringBuilder(13);
            while (true)
            {
                var index = b & 31;
                sb.Append(_base32[index]);
                b = b >> 5;
                if (b == 0)
                {
                    break;
                }
            }

            return sb.ToString();
        }

    }

}
