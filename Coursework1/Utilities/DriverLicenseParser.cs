using Coursework1.Data;

namespace Coursework1.Utilities;

public static class DriverLicenseParser
{
    public static bool TryParse(string input, out DriverLicense output, out string error)
    {
        output = default;
        error = string.Empty;
        
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