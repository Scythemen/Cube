using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Utility.TextJson
{
    public class StringToBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }
            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out bool b1, out var bytesConsumed) && span.Length == bytesConsumed)
                {
                    return b1;
                }

                if (bool.TryParse(reader.GetString(), out var b2))
                {
                    return b2;
                }

                if (string.IsNullOrWhiteSpace(reader.GetString()))
                {
                    return false;
                }
            }

            //string value = reader.GetString();
            //string chkValue = value.ToLower();
            //if (chkValue.Equals("true") || chkValue.Equals("yes") || chkValue.Equals("1"))
            //{
            //    return true;
            //}
            //if (value.ToLower().Equals("false") || chkValue.Equals("no") || chkValue.Equals("0"))
            //{
            //    return false;
            //}

            return false;
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
