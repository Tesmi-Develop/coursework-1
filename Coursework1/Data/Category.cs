using System.Text.Json.Serialization;
using Coursework1.JsonConverters;

namespace Coursework1.Data;

[JsonConverter(typeof(VehicleCategoryConverter))]
[Flags]
public enum VehicleCategory
{
    None = 0,
    A = 1 << 0,
    B = 1 << 1,
    C = 1 << 2,
    D = 1 << 3,
    M = 1 << 4,
    BE = 1 << 5,
    CE = 1 << 6
}