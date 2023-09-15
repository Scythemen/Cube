using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Cube.Utility
{
    public static class BytesHelper
    {
        public static byte Cs(ArraySegment<byte> b, int startIndex = 0, int length = -1)
        {
            length = length < 0 ? b.Count : length;
            if (length == 0 || b == null || b.Count < 1)
            {
                return 0;
            }

            startIndex = startIndex < 0 ? 0 : startIndex;
            if (startIndex >= b.Count || (startIndex + length) > b.Count)
            {
                throw new IndexOutOfRangeException();
            }

            int sum = 0;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                sum += b[i];
            }

            return (byte)(sum & 0xff);
        }

        public static byte Cs(this ReadOnlySequence<byte> b)
        {
            int cs = 0;
            foreach (var item in b)
            {
                foreach (var n in item.Span)
                {
                    cs += n;
                }
            }

            return (byte)cs;
        }

        public static byte Cs(IEnumerable<byte> list, int startIndex = 0, int length = -1)
        {
            length = length < 0 ? list.Count() : length;
            if (length == 0 || list == null || list.Count() < 1)
            {
                return 0;
            }

            startIndex = startIndex < 0 ? 0 : startIndex;
            if (startIndex >= list.Count() || (startIndex + length) > list.Count())
            {
                throw new IndexOutOfRangeException();
            }

            var i = 0;
            var n = 0;
            int sum = 0;
            var em = list.GetEnumerator();
            while (em.MoveNext())
            {
                if (n >= length)
                {
                    break;
                }

                if (i >= startIndex)
                {
                    n++;
                    sum += em.Current;
                }

                i++;
            }

            return (byte)(sum & 0xff);
        }

        public static byte[] Slice(this byte[] arr, int index, int? count = null)
        {
            if (index < 0 || index > arr.Length || (count.HasValue && (index + count.Value) > arr.Length))
                throw new IndexOutOfRangeException();

            if (!count.HasValue) count = arr.Length - index;

            byte[] b = new byte[count.Value];
            for (int i = 0; i < count.Value; i++)
            {
                b[i] = arr[index + i];
            }

            return b;
        }

        public static (string s1, string s2) Split2(this string str, string separator, StringSplitOptions opt = StringSplitOptions.None)
        {
            if (string.IsNullOrWhiteSpace(str)) return (str, null);

            var s = str.Split(separator, opt);
            return (s[0], s.Length > 1 ? s[1] : null);
        }

        public static string ReverseHex(this string hex)
        {
            if (hex.Length % 2 != 0) hex += "0";
            var b = new StringBuilder();
            for (int i = hex.Length - 1; i >= 0; i = i - 2)
            {
                b.Append($"{hex[i - 1]}{hex[i]}");
            }

            return b.ToString();
        }

        public static string InsertSpace(this string hex)
        {
            if (string.IsNullOrEmpty(hex)) return hex;

            var sb = new StringBuilder(hex.Length + (hex.Length >> 1) + 1);
            for (var i = 0; i < hex.Length; i++)
            {
                sb.Append(hex[i]);
                if (i + 1 < hex.Length)
                {
                    sb.Append(hex[i + 1]);
                    if (i + 1 != hex.Length - 1)
                    {
                        sb.Append(' ');
                    }

                    i++;
                }
            }

            return sb.ToString();
        }

        public static byte[] HexToBytes(this string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
            {
                return Array.Empty<byte>();
            }

            if (hexString.Length % 2U != 0)
            {
                var s = '0' + hexString;
                return Convert.FromHexString(s);
            }
            else
            {
                return Convert.FromHexString(hexString);
            }
        }


        private static readonly string[] HexArray = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };

        public static string ToHex(this byte[] bytes, int start = -1, int length = -1)
        {
            if (bytes == null || bytes.Length < 1) return string.Empty;

            if (start >= bytes.Length)
            {
                throw new ArgumentOutOfRangeException("start");
            }

            if (length >= bytes.Length || start + length > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            start = start < 0 ? 0 : start;
            length = length < 0 ? bytes.Length : length;
            var sb = new StringBuilder(length << 1);
            for (int i = start; i < length; i++)
            {
                sb.Append(HexArray[(bytes[i] & 0xf0) >> 4]);
                sb.Append(HexArray[bytes[i] & 0x0f]);
            }

            return sb.ToString();
        }

        public static string ToHex(this byte bytes)
        {
            return HexArray[(bytes & 0xf0) >> 4] + HexArray[bytes & 0x0f];
        }

        public static string ToHex(this ArraySegment<byte> bytes, int startIndex = 0, int length = -1)
        {
            if (bytes == null || bytes.Count < 1) return string.Empty;

            if (length < 0)
            {
                length = bytes.Count;
            }

            var sb = new StringBuilder(length * 2);
            for (int i = startIndex, n = 0; i < bytes.Count && n < length; i++, n++)
            {
                var b = bytes[i];
                sb.Append(HexArray[(b & 0xf0) >> 4] + HexArray[b & 0x0f]);
            }

            return sb.ToString();
        }


        public static int Bcd2Int(this byte b)
        {
            return b & 0x0f + ((b & 0xf0) >> 4) * 10;
        }

        public static byte[] Add33(this byte[] b)
        {
            for (int i = 0; i < b.Length; i++) b[i] = (byte)(b[i] + 0x33);
            return b;
        }

        public static List<byte> Add33(this List<byte> b)
        {
            for (int i = 0; i < b.Count; i++) b[i] = (byte)(b[i] + 0x33);
            return b;
        }

        public static byte[] Sub33(this byte[] b)
        {
            for (int i = 0; i < b.Length; i++) b[i] = (byte)(b[i] - 0x33);
            return b;
        }


        public static string Bcd2Str(this byte[] b, int decimalPlace = 0)
        {
            if (decimalPlace <= 0) return b.ToHex();

            var v = b.ToHex();

            return v.Insert(v.Length - decimalPlace, ".");
        }

        public static byte[] Bcd2Bytes(this string bcd)
        {
            // 4-bit BCD
            if (string.IsNullOrEmpty(bcd)) return Array.Empty<byte>();

            bcd = bcd.Replace(".", "");

            if (bcd.Length % 2U == 1)
            {
                bcd = "0" + bcd;
            }

            return HexToBytes(bcd);
        }

        public static long ToLong(this byte[] bs)
        {
            if (bs == null || bs.Length < 1)
            {
                return 0;
            }

            long n = 0;
            for (int i = bs.Length - 1, j = 0; i >= 0; i--, j++)
            {
                n = n | (uint)(bs[i] << (j << 3));
            }

            return n;
        }
    }
}