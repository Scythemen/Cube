using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cube.Utility.TextJson
{
    public class ObjectToStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            //if (reader.TokenType == JsonTokenType.String)
            //{
            //    ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;

            //    return span.ToString();
            //}
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.TryGetInt64(out long lg) ? lg.ToString() : reader.GetDouble().ToString();
            }

            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                return document.RootElement.Clone().ToString();
            }

        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

    }

}
