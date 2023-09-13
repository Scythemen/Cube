using System;
using System.Text;

namespace Cube.Utility
{
    public static class TextHelper
    {
        public static string Md5(byte[] input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(input);

                StringBuilder sb = new StringBuilder(hashBytes.Length * 2);
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }


        /// <summary>
        /// 半角转全角(SBC case)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSBC(string str)
        {
            char[] c = str.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }

                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }

            return new string(c);
        }


        /// <summary>
        /// 全角转半角 (SBC case)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToDBC(string str)
        {
            char[] c = str.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }

                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }

            return new string(c);
        }


        public static (string s1, string s2) Split2(this string str, char separator, StringSplitOptions opt = StringSplitOptions.None)
        {
            if (string.IsNullOrWhiteSpace(str)) return (str, null);

            var s = str.Split(separator, opt);
            return (s[0], s.Length > 1 ? s[1] : null);
        }

        public static (string s1, string s2, string s3) Split3(this string str, string separator, StringSplitOptions opt = StringSplitOptions.None)
        {
            if (string.IsNullOrWhiteSpace(str)) return (str, null, null);

            var s = str.Split(separator, opt);
            return (s[0], s.Length > 1 ? s[1] : null, s.Length > 2 ? s[2] : null);
        }

        public static (string s1, string s2, string s3) Split3(this string str, char separator, StringSplitOptions opt = StringSplitOptions.None)
        {
            if (string.IsNullOrWhiteSpace(str)) return (str, null, null);

            var s = str.Split(separator, opt);
            return (s[0], s.Length > 1 ? s[1] : null, s.Length > 2 ? s[2] : null);
        }

        public static (string s1, string s2, string s3, string s4) Split4(this string str, string separator,
            StringSplitOptions opt = StringSplitOptions.None)
        {
            if (string.IsNullOrWhiteSpace(str)) return (str, null, null, null);

            var s = str.Split(separator, opt);
            return (s[0], s.Length > 1 ? s[1] : null, s.Length > 2 ? s[2] : null, s.Length > 3 ? s[3] : null);
        }

        public static (string s1, string s2, string s3, string s4, string s5) Split5(this string str, string separator,
            StringSplitOptions opt = StringSplitOptions.None)
        {
            if (string.IsNullOrWhiteSpace(str)) return (str, null, null, null, null);

            var s = str.Split(separator, opt);
            return (s[0], s.Length > 1 ? s[1] : null, s.Length > 2 ? s[2] : null, s.Length > 3 ? s[3] : null, s.Length > 4 ? s[4] : null);
        }

        public static (string s1, string s2, string s3, string s4, string s5, string s6) Split6(this string str, string separator,
            StringSplitOptions opt = StringSplitOptions.None)
        {
            if (string.IsNullOrWhiteSpace(str)) return (str, null, null, null, null, null);

            var s = str.Split(separator, opt);
            return (s[0], s.Length > 1 ? s[1] : null, s.Length > 2 ? s[2] : null, s.Length > 3 ? s[3] : null, s.Length > 4 ? s[4] : null,
                s.Length > 5 ? s[5] : null);
        }
    }

    /// <summary>
    /// 一些常用的正则表达式, Regex.IsMatch("")
    /// </summary>
    public struct RegularExp
    {
        public const string Chinese = @"^[\u4E00-\u9FA5\uF900-\uFA2D]+$";
        public const string Color = "^#[a-fA-F0-9]{6}";

        public const string Date =
            @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$";

        public const string Time = @"^(([0-1]?[0-9])|([2][0-3])):([0-5]?[0-9])(:([0-5]?[0-9]))?$";

        public const string DateTime =
            @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-)) (20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d$";

        public const string Email = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";
        public const string Float = @"^(-?\d+)(\.\d+)?$";
        public const string ImageFormat = @"\.(?i:jpg|bmp|gif|ico|pcx|jpeg|tif|png|raw|tga)$";
        public const string Integer = @"^-?\d+$";

        public const string IP =
            @"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$";

        public const string Letter = "^[A-Za-z]+$";
        public const string LowerLetter = "^[a-z]+$";

        /// <summary>
        /// 验证负浮点数
        /// </summary>
        public const string MinusFloat = @"^(-(([0-9]+\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\.[0-9]+)|([0-9]*[1-9][0-9]*)))$";

        /// <summary>
        /// 验证负整数
        /// </summary>
        public const string MinusInteger = "^-[0-9]*[1-9][0-9]*$";

        public const string Mobile = "^0{0,1}1[0-9]{10}$"; // 1[3,4,5,6,7,8,9]\d{9}
        public const string NumericOrLetterOrChinese = @"^[A-Za-z0-9\u4E00-\u9FA5\uF900-\uFA2D]+$";
        public const string Numeric = "^[0-9]+$";
        public const string Hex = "^[A-Fa-f0-9]+$";
        public const string NumericOrLetter = "^[A-Za-z0-9]+$";
        public const string NumericOrLetterOrUnderline = @"^\w+$";

        /// <summary>
        /// 验证正浮点数
        /// </summary>
        public const string PlusFloat = @"^(([0-9]+\.[0-9]*[1-9][0-9]*)|([0-9]*[1-9][0-9]*\.[0-9]+)|([0-9]*[1-9][0-9]*))$";

        /// <summary>
        /// 正整数
        /// </summary>
        public const string PlusInteger = "^[0-9]*[1-9][0-9]*$";

        public const string Telephone = @"(\d+-)?(\d{4}-?\d{7}|\d{3}-?\d{8}|^\d{7,8})(-\d+)?";

        /// <summary>
        ///  验证非负浮点数（正浮点数 和 0）
        /// </summary>
        public const string UnMinusFloat = @"^\d+(\.\d+)?$";

        /// <summary>
        /// 验证非负整数（正整数 和 0）
        /// </summary>
        public const string UnMinusInteger = @"\d+$";

        /// <summary>
        /// 验证非正浮点数（负浮点数 和 0）
        /// </summary>
        public const string UnPlusFloat = @"^((-\d+(\.\d+)?)|(0+(\.0+)?))$";

        /// <summary>
        /// 验证非正整数（负整数 和 0） 
        /// </summary>
        public const string UnPlusInteger = @"^((-\d+)|(0+))$";

        public const string UpperLetter = "^[A-Z]+$";
        public const string Url = @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
    }
}