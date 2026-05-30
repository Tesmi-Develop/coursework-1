using System.Text.Json;
using System.Text.Json.Serialization;
using Coursework1.Data;
using Coursework1.Utilities;

namespace Coursework1.JsonConverters;

public class VehicleCategoryConverter : JsonConverter<VehicleCategory>
{
    public override VehicleCategory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Ожидалась строка категорий.");
        
        var input = reader.GetString();
        if (string.IsNullOrWhiteSpace(input))
            return default;
        
        var categories = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
        VehicleCategory result = default;

        foreach (var item in categories)
        {
            var category = item.Trim();

            if (Enum.TryParse<VehicleCategory>(category, ignoreCase: true, out var parsedCategory))
                result |= parsedCategory;
            else
                throw new JsonException($"Не удалось распознать категорию {category}");
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, VehicleCategory value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}