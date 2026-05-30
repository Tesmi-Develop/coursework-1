using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Coursework1.JsonConverters;

namespace Coursework1.Data;

[JsonConverter(typeof(DriverLicenseConverter))]
public readonly record struct DriverLicense : IComparable<DriverLicense>
{
    public static readonly DriverLicense Invalid = new(-1, -1);

    public int Series => (int)(_mask >> 32);
    public int Number => (int)(_mask & 0xFFFFFFFF);
    
    private readonly long _mask;

    public DriverLicense(int series, int number)
    {
        _mask = (long)series << 32 | (uint)number; 
    }

    public override string ToString()
    {
        return $"{Series}-{Number}";
    }

    public int CompareTo(DriverLicense other)
    {
        return _mask.CompareTo(other._mask);
    }
}