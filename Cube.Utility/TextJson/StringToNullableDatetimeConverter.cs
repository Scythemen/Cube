using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Utility.TextJson
{
    public class StringToNullableDatetimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out DateTime b1, out var bytesConsumed) && span.Length == bytesConsumed)
                {
                    return b1;
                }

                if (DateTime.TryParse(reader.GetString(), out var b2))
                {
                    return b2;
                }

                if (string.IsNullOrWhiteSpace(reader.GetString()))
                {
                    return null;
                }
            }

            return reader.GetDateTime();
        }


        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString("O"));
            }
            else
            {
                writer.WriteStringValue(string.Empty);
            }
        }
    }
}
