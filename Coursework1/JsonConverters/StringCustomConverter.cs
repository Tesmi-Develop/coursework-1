using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coursework1.JsonConverters;

public class StringCustomConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Ожидалась строка");
        
        var result = reader.GetString();

        return string.IsNullOrEmpty(result) ? throw new JsonException("Строка не может быть пустой") : result;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}