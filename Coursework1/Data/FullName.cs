using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Coursework1.JsonConverters;

namespace Coursework1.Data;

[JsonConverter(typeof(FullNameConverter))]
public readonly record struct FullName
{
    public string LastName { get; init; }
    public string FirstName { get; init; }
    public string MiddleName { get; init; }
    
    public FullName(string lastName, string firstName, string middleName)
    {
        LastName = lastName;
        FirstName = firstName;
        MiddleName = middleName;
    }

    public override string ToString()
    {
        return $"{LastName} {FirstName} {MiddleName}";
    }
}