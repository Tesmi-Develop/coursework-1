using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coursework1.JsonConverters;

public class IntCustomConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
            if (reader.TryGetInt32(out int value))
                return value;
        
        throw new JsonException("Ожидалось число");
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}