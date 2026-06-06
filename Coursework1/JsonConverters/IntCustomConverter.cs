using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coursework1.JsonConverters;

public class IntCustomConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out var value)) 
            throw new JsonException("Ожидалось число");
        
        if (value <= 0)
            throw new JsonException("Число должно быть больше 0");
        
        return value;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}