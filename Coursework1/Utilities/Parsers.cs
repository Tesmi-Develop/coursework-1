using System.Text.RegularExpressions;
using Coursework1.Data;

namespace Coursework1.Utilities;

public static class Parsers
{
    public static bool TryParseFullName(string lastName, string firstName, string middleName, out FullName name, out string error)
    {
        lastName = lastName.Trim();
        firstName = firstName.Trim();
        middleName = middleName.Trim();
        
        error = string.Empty;
        name = default;
        const string pattern = "^[А-ЯЁ][а-яё]{1,}$";
        
        var isLastValid = Regex.IsMatch(lastName, pattern);
        var isFirstValid = Regex.IsMatch(firstName, pattern);
        var isMiddleValid = Regex.IsMatch(middleName, pattern);
        
        if (isLastValid && isFirstValid && isMiddleValid)
        {
            name = new FullName(lastName, firstName, middleName);
            return true;
        }

        error = "Некорректное ФИО";
        return false;
    }
    
    public static bool TryParseLicense(string input, out DriverLicense output, out string error)
    {
        input = input.Trim();
        
        output = default;
        error = string.Empty;
        
        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Введите данные ВУ";
            return false;
        }
        
        var split = input.Split('-', StringSplitOptions.RemoveEmptyEntries);
        
        if (split.Length != 2)
        {
            error = "Формат ВУ: 0000-000000 (серия-номер)";
            return false;
        }

        var serialStr = split[0];
        var numberStr = split[1];
        
        if (serialStr.Length != 4 || !int.TryParse(serialStr, out var serial))
        {
            error = "Серия должна состоять из 4 цифр";
            return false;
        }
        
        if (numberStr.Length != 6 || !int.TryParse(numberStr, out var driverNumber))
        {
            error = "Номер должен состоять из 6 цифр";
            return false;
        }
        
        if (serial <= 0 || driverNumber <= 0)
        {
            error = "Данные ВУ не могут быть нулевыми";
            return false;
        }

        output = new DriverLicense(serial, driverNumber);
        return true;
    }
}