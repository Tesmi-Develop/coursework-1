using System.Text.Json;
using System.Text.Json.Serialization;
using Coursework1.Data;

namespace Coursework1.JsonConverters;

public class FormattedDateConverter : JsonConverter<FormattedDate>
{
    public override FormattedDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Ожидалась строка даты.");
        
        var input = reader.GetString() ?? string.Empty;
        if (!FormattedDate.TryParse(input, out var result, out var errorMessage))
            throw new JsonException(errorMessage);

        return result;
    }

    public override void Write(Utf8JsonWriter writer, FormattedDate value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}