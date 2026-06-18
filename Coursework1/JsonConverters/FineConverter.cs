using System.Text.Json;
using System.Text.Json.Serialization;
using Coursework1.Data;

namespace Coursework1.JsonConverters;

public class FineConverter : JsonConverter<Fine>
{
    private static readonly string[] RequiredFields = { "License", "Article", "Price", "Date" };
    
    private static readonly UIntCustomConverter UIntConverter = new();
    private static readonly StringCustomConverter _stringConverter = new();

    public override Fine Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Ожидался объект штрафа.");
        
        DriverLicense? license = null;
        string? article = null;
        uint? price = null;
        FormattedDate? date = null;
        
        var fieldsFound = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Ожидалось название свойства");

            var propertyName = reader.GetString();
            
            if (!RequiredFields.Contains(propertyName))
                throw new JsonException($"Обнаружено лишнее или неверное поле: '{propertyName}'. Разрешены только: License, Article, Price, Date");

            reader.Read();

            switch (propertyName)
            {
                case "License":
                    license = JsonSerializer.Deserialize<DriverLicense>(ref reader, options);
                    fieldsFound++;
                    break;
                    
                case "Article":
                    article = _stringConverter.Read(ref reader, typeof(string), options);
                    fieldsFound++;
                    break;
                    
                case "Price":
                    price = UIntConverter.Read(ref reader, typeof(uint), options);
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
        
        _stringConverter.Write(writer, value.Article, options);
        
        writer.WritePropertyName("Price");
        
        UIntConverter.Write(writer, value.Price, options);
        
        writer.WritePropertyName("Date");
        JsonSerializer.Serialize(writer, value.Date, options);
        
        writer.WriteEndObject();
    }
}