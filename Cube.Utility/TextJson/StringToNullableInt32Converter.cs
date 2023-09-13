using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Utility.TextJson
{
    public class StringToNullableInt32Converter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out int b1, out var bytesConsumed) && span.Length == bytesConsumed)
                {
                    return b1;
                }

                if (int.TryParse(reader.GetString(), out var b2))
                {
                    return b2;
                }
                else
                {
                    return null;
                }
            }

            return reader.GetInt32();
        }


        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteStringValue(string.Empty);
            }
        }
    }
}
