using System.Text.Json;
using System.Text.Json.Serialization;
using Coursework1.Data;
using Coursework1.Utilities;

namespace Coursework1.JsonConverters;

public class DriverLicenseConverter : JsonConverter<DriverLicense>
{
    public override DriverLicense Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Ожидалась строка ВУ.");
        
        var input = reader.GetString() ?? string.Empty;
        if (!Parsers.TryParseLicense(input, out var output, out var errorMessage))
            throw new JsonException(errorMessage);
        
        return output;
    }

    public override void Write(Utf8JsonWriter writer, DriverLicense value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}