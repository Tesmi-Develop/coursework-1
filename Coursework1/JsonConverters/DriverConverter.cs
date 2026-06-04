using System.Text.Json;
using System.Text.Json.Serialization;
using Coursework1.Data;

namespace Coursework1.JsonConverters;

public class DriverConverter : JsonConverter<Driver>
{
    private static readonly string[] RequiredFields = { "License", "FullName", "Categories" };

    public override Driver Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Ожидался объект водителеля.");
        
        DriverLicense? license = null;
        FullName? fullName = null;
        VehicleCategory? categories = null;
        
        var fieldsFound = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Ожидалось название свойства.");

            var propertyName = reader.GetString();
            
            if (!RequiredFields.Contains(propertyName))
                throw new JsonException($"Обнаружено лишнее или неверное поле: '{propertyName}'. Разрешены только: License, FullName, Categories.");

            reader.Read();

            switch (propertyName)
            {
                case "License":
                    license = JsonSerializer.Deserialize<DriverLicense>(ref reader, options);
                    fieldsFound++;
                    break;
                case "FullName":
                    fullName = JsonSerializer.Deserialize<FullName>(ref reader, options);
                    fieldsFound++;
                    break;
                case "Categories":
                    categories = JsonSerializer.Deserialize<VehicleCategory>(ref reader, options);
                    fieldsFound++;
                    break;
            }
        }
        
        if (fieldsFound != 3)
            throw new JsonException($"Неверная структура объекта. Найдено полей: {fieldsFound}, а должно быть 3 (License, FullName, Categories).");

        return new Driver
        {
            License = license!.Value,
            FullName = fullName!.Value,
            Categories = categories!.Value
        };
    }

    public override void Write(Utf8JsonWriter writer, Driver value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        writer.WritePropertyName("License");
        JsonSerializer.Serialize(writer, value.License, options);
        
        writer.WritePropertyName("FullName");
        JsonSerializer.Serialize(writer, value.FullName, options);
        
        writer.WritePropertyName("Categories");
        JsonSerializer.Serialize(writer, value.Categories, options);
        
        writer.WriteEndObject();
    }
}