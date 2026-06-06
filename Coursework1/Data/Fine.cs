using System.Text.Json.Serialization;
using Coursework1.JsonConverters;

namespace Coursework1.Data;

[JsonConverter(typeof(FineConverter))]
public record struct Fine
{
    public DriverLicense License { get; init; }
    
    [JsonConverter(typeof(StringCustomConverter))]
    public string Article { get; set; }
    
    [JsonConverter(typeof(IntCustomConverter))]
    public int Price { get; set; }
    public FormattedDate Date { get; set; }
}