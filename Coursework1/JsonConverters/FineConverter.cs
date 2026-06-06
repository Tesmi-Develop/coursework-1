using System.Text.Json;
using System.Text.Json.Serialization;
using Coursework1.Data;

namespace Coursework1.JsonConverters;

public class FineConverter : JsonConverter<Fine>
{
    private static readonly string[] RequiredFields = { "License", "Article", "Price", "Date" };

    public override Fine Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Ожидался объект штрафа.");
        
        DriverLicense? license = null;
        string? article = null;
        int? price = null;
        FormattedDate? date = null;
        
        var fieldsFound = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Ожидалось название свойства.");

            var propertyName = reader.GetString();
            
            if (!RequiredFields.Contains(propertyName))
                throw new JsonException($"Обнаружено лишнее или неверное поле: '{propertyName}'. Разрешены только: License, Article, Price, Date.");

            reader.Read();

            switch (propertyName)
            {
                case "License":
                    license = JsonSerializer.Deserialize<DriverLicense>(ref reader, options);
                    fieldsFound++;
                    break;
                case "Article":
                    article = JsonSerializer.Deserialize<string>(ref reader, options);
                    fieldsFound++;
                    break;
                case "Price":
                    price = JsonSerializer.Deserialize<int>(ref reader, options);
                    fieldsFound++;
                    break;
                case "Date":
                    date = JsonSerializer.Deserialize<FormattedDate>(ref reader, options);
                    fieldsFound++;
                    break;
            }
        }
        
        if (fieldsFound != 4)
            throw new JsonException($"Неверная структура объекта. Найдено полей: {fieldsFound}, а должно быть 4 (License, Article, Price, Date).");

        return new Fine
        {
            License = license!.Value,
            Article = article!,
            Price = price!.Value,
            Date = date!.Value
        };
    }

    public override void Write(Utf8JsonWriter writer, Fine value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WritePropertyName("License");
        JsonSerializer.Serialize(writer, value.License, options);
        
        writer.WritePropertyName("Article");
        writer.WriteStringValue(value.Article);
        
        writer.WritePropertyName("Price");
        writer.WriteNumberValue(value.Price);
        
        writer.WritePropertyName("Date");
        JsonSerializer.Serialize(writer, value.Date, options);
        
        writer.WriteEndObject();
    }
}