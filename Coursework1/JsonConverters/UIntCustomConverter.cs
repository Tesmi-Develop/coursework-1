using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coursework1.JsonConverters;

public class UIntCustomConverter : JsonConverter<uint>
{
    public override uint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetUInt32(out var value)) 
            throw new JsonException("Ожидалось число");
        
        return value;
    }

    public override void Write(Utf8JsonWriter writer, uint value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}