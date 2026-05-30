using System.Text.Json;
using System.Text.Json.Serialization;
using Coursework1.Data;
using Coursework1.Utilities;

namespace Coursework1.JsonConverters;

public class FullNameConverter : JsonConverter<FullName>
{
    public override FullName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var input = jsonDoc.RootElement.GetString() ?? string.Empty;

        var split = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 3)
            throw new JsonException("Некорректное ФИО");
        
        if (!Parsers.TryParseFullName(split[0], split[1], split[2], out var output, out var errorMessage))
            throw new JsonException(errorMessage);
        
        return output;
    }

    public override void Write(Utf8JsonWriter writer, FullName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}