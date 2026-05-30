using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Coursework1.JsonConverters;

namespace Coursework1.Data;

[JsonConverter(typeof(FormattedDateConverter))]
public readonly record struct FormattedDate(DateTime Value)
{
    private static readonly string[] MonthNames = 
    { 
        "янв", "фев", "мар", "апр", "мая", "июн", 
        "июл", "авг", "сен", "окт", "ноя", "дек" 
    };

    public override string ToString()
    {
        var month = MonthNames[Value.Month - 1];
        return $"{Value.Day:D2} {month} {Value.Year}";
    }

    public static bool TryParse(string input, out FormattedDate result, out string error)
    {
        result = default;
        error = string.Empty;
        
        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Строка даты пуста";
            return false;
        }

        var parts = input.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3)
        {
            error = "Ожидался формат 'DD янв YYYY'";
            return false;
        }

        if (!int.TryParse(parts[0], out var day) || !int.TryParse(parts[2], out var year))
        {
            error = "Не удалось распознать день или год";
            return false;
        }
        
        var monthStr = parts[1].TrimEnd('.');
        var monthIndex = Array.IndexOf(MonthNames, monthStr) + 1;

        if (monthIndex <= 0)
        {
            if (monthStr == "май") monthIndex = 5;
            else
            {
                error = $"Неизвестный месяц: {monthStr}";
                return false;
            }
        }

        result = new FormattedDate(new DateTime(year, monthIndex, day));
        return true;
    }

    public static implicit operator FormattedDate(DateTime dt) => new(dt);
    public static implicit operator DateTime(FormattedDate dt) => dt.Value;
}