using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Cube.Utility
{
    public static class StringExtensions
    {
        public static string ToCamelCaseName(this string name, CultureInfo culture = null)
        {
            return string.IsNullOrEmpty(name) ? name : char.ToLower(name[0], culture ?? CultureInfo.InvariantCulture) + name.Substring(1);
        }

        public static string ToSnakeCaseName(this string name, CultureInfo culture = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var builder = new StringBuilder(name.Length + Math.Min(2, name.Length / 5));
            var previousCategory = default(UnicodeCategory?);

            for (var currentIndex = 0; currentIndex < name.Length; currentIndex++)
            {
                var currentChar = name[currentIndex];
                if (currentChar == '_')
                {
                    builder.Append('_');
                    previousCategory = null;
                    continue;
                }

                var currentCategory = char.GetUnicodeCategory(currentChar);
                switch (currentCategory)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                        if (previousCategory == UnicodeCategory.SpaceSeparator ||
                            previousCategory == UnicodeCategory.LowercaseLetter ||
                            previousCategory != UnicodeCategory.DecimalDigitNumber &&
                            previousCategory != null &&
                            currentIndex > 0 &&
                            currentIndex + 1 < name.Length &&
                            char.IsLower(name[currentIndex + 1]))
                        {
                            builder.Append('_');
                        }

                        currentChar = char.ToLower(currentChar, culture ?? CultureInfo.InvariantCulture);
                        break;

                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (previousCategory == UnicodeCategory.SpaceSeparator)
                        {
                            builder.Append('_');
                        }
                        break;

                    default:
                        if (previousCategory != null)
                        {
                            previousCategory = UnicodeCategory.SpaceSeparator;
                        }
                        continue;
                }

                builder.Append(currentChar);
                previousCategory = currentCategory;
            }

            return builder.ToString();
        }

        public static string FormatNamed(this string fmt, object obj, IFormatProvider formatProvider = null)
        {
            //   http://www.hanselman.com/blog/CommentView.aspx?guid=fde45b51-9d12-46fd-b877-da6172fe1791
            if (string.IsNullOrWhiteSpace(fmt) || obj == null)
            {
                return fmt;
            }

            StringBuilder sb = new StringBuilder();
            Type type = obj.GetType();
            Regex reg = new Regex(@"({)([^}]+)(})", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(fmt);
            int startIndex = 0;
            foreach (Match m in mc)
            {
                Group g = m.Groups[2]; //it's second in the match between { and }  
                int length = g.Index - startIndex - 1;
                sb.Append(fmt.Substring(startIndex, length));

                string toGet = String.Empty;
                string toFormat = String.Empty;
                int formatIndex = g.Value.IndexOf(":"); //formatting would be to the right of a :  
                if (formatIndex == -1) //no formatting, no worries  
                {
                    toGet = g.Value;
                }
                else //pickup the formatting  
                {
                    toGet = g.Value.Substring(0, formatIndex);
                    toFormat = g.Value.Substring(formatIndex + 1);
                }

                //first try properties  
                PropertyInfo retrievedProperty = type.GetProperty(toGet);
                Type retrievedType = null;
                object retrievedObject = null;
                if (retrievedProperty != null)
                {
                    retrievedType = retrievedProperty.PropertyType;
                    retrievedObject = retrievedProperty.GetValue(obj, null);
                }
                else //try fields  
                {
                    FieldInfo retrievedField = type.GetField(toGet);
                    if (retrievedField != null)
                    {
                        retrievedType = retrievedField.FieldType;
                        retrievedObject = retrievedField.GetValue(obj);
                    }
                }

                if (retrievedType != null) //Cool, we found something  
                {
                    string result = String.Empty;
                    if (toFormat == String.Empty) //no format info  
                    {
                        result = retrievedType.InvokeMember("ToString",
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                            , null, retrievedObject, null) as string;
                    }
                    else //format info  
                    {
                        result = retrievedType.InvokeMember("ToString",
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                            , null, retrievedObject, new object[] { toFormat, formatProvider }) as string;
                    }
                    sb.Append(result);
                }
                else //didn't find a property with that name, so be gracious and put it back  
                {
                    sb.Append("{");
                    sb.Append(g.Value);
                    sb.Append("}");
                }
                startIndex = g.Index + g.Length + 1;
            }
            if (startIndex < fmt.Length) //include the rest (end) of the string  
            {
                sb.Append(fmt.Substring(startIndex));
            }
            return sb.ToString();
        }




    }
}

