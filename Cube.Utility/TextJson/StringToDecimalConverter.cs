using System;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Utility.TextJson
{
    public class StringToDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
                if (Utf8Parser.TryParse(span, out decimal b1, out var bytesConsumed) && span.Length == bytesConsumed)
                {
                    return b1;
                }

                if (decimal.TryParse(reader.GetString(), out var b2))
                {
                    return b2;
                }
                else
                {
                    return default(decimal);
                }
            }

            return reader.GetDecimal();
        }


        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
