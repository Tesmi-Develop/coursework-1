using System.IO;
using System.Text.Json;
using Coursework1.Collections;
using Coursework1.Data;

namespace Coursework1.Databases;

public class DriverDatabase
{
    public int Count => _drivers.Count;
    public event Action<string>? LogMessage;

    private HashTable<DriverLicense, int> _indexesByLicense = new();
    private readonly DynamicArray<Driver> _drivers = [];

    private void Log(string message)
    {
        LogMessage?.Invoke(message);
    }

    public bool TrySetSettings(int capacity)
    {
        if (_drivers.Count > 0)
            return false;
        
        var oldHashTable = _indexesByLicense;
        _indexesByLicense = new HashTable<DriverLicense, int>(capacity);
        
        Log($"[INIT] Успешно применены настройки для ХТ: capacity = {capacity}");
        return true;
    }

    public bool Has(DriverLicense license)
    {
        return _indexesByLicense.ContainsKey(license);
    }

    public bool TryAdd(Driver driver)
    {
        Log($"[ADD] Попытка добавления: {driver.License}");
        
        if (_indexesByLicense.ContainsKey(driver.License))
        {
            Log($"[FAIL] Ключ {driver.License} уже существует.");
            return false;
        }
        
        var newIndex = _drivers.Count;
        
        if (_indexesByLicense.Add(driver.License, newIndex))
        {
            _drivers.Add(driver);
            Log($"[SUCCESS] Добавлен: {driver.License}, ID: {newIndex}");
            Log(_indexesByLicense.PrintDebugInfo());
            return true;
        }
        
        Log("[ERROR] Ошибка добавления (таблица полна или конфликт).");
        return false;
    }

    public bool Remove(DriverLicense license, out Driver removedDriver, out Driver? replacer)
    {
        removedDriver = default;
        replacer = default;
        
        if (!_indexesByLicense.Remove(license, out var index))
        {
            Log($"[FAIL] Ключ {license} не найден.");
            return false;
        }

        var lastIndex = _drivers.Count - 1;
        removedDriver = _drivers[index];

        if (index != lastIndex)
        {
            var lastDriver = _drivers[lastIndex];
            
            _indexesByLicense[lastDriver.License] = index;
            _drivers[index] = lastDriver;
            replacer = _drivers[index];
        }
        
        _drivers.RemoveAt(lastIndex);
        
        Log($"[SUCCESS] Удален: {removedDriver.License}");
        Log(_indexesByLicense.PrintDebugInfo());
        return true;
    }
    
    public bool TryFind(DriverLicense license, out Driver driver, out int steps)
    {
        if (!_indexesByLicense.Find(license, out var index, out steps))
        {
            Log("[NOT FOUND] Не найден.");
            driver = default;
            return false;
        }

        driver = _drivers[index];
        Log($"[FOUND] Найден: {driver.FullName}");
        return true;
    }

    public bool TryFind(DriverLicense license, out Driver driver)
    {
        if (!_indexesByLicense.TryGetValue(license, out var index))
        {
            Log("[NOT FOUND] Не найден.");
            driver = default;
            return false;
        }

        driver = _drivers[index];
        Log($"[FOUND] Найден: {driver.FullName}");
        return true;
    }

    public IEnumerable<Driver> GetAllDrivers()
    {
        return _drivers;
    }
    
    public bool TryExport(string filePath, out string error)
    {
        error = string.Empty;
        string output;
        
        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        
        try
        {
            output = JsonSerializer.Serialize(_drivers.ToArray(), options);
        }
        catch (Exception e)
        {
            error = $"Ошибка при сериализации данных в JSON: {e}";
            return false;
        }

        try
        {
            File.WriteAllText(filePath, output);
        }
        catch (Exception e)
        {
            error = $"Ошибка при записи данных в файл: {e}";
            return false;
        }
        
        return true;
    }

    public bool TryImport(string filePath, out string error)
    {
        error = string.Empty;

        if (!File.Exists(filePath))
        {
            error = $"Файл по пути '{filePath}' не найден";
            return false;
        }

        string fileContent;
        
        try
        {
            fileContent = File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            error = $"Ошибка при открытии файла: {e.Message}";
            return false;
        }
        
        Driver[] drivers;

        try
        {
            drivers = JsonSerializer.Deserialize<Driver[]>(fileContent) ?? [];
        }
        catch (JsonException e)
        {
            error = $"Ошибка при десериализации JSON: строка: {e.LineNumber + 1}, позиция: {e.BytePositionInLine}: {e.Message}";
            return false;
        }
        
        var localHashTable = new HashTable<DriverLicense, int>();
        var localArray = new DynamicArray<Driver>();

        foreach (var driver in drivers)
        {
            if (localHashTable.ContainsKey(driver.License))
            {
                error = $"ВУ: {driver.License} встретилось несколько раз";
                return false;
            }
            
            localArray.Add(driver);
            localHashTable.Add(driver.License, localArray.Count - 1);
        }
        
        _drivers.Clear();
        _indexesByLicense.Clear();

        foreach (var driver in localArray)
        {
            _drivers.Add(driver);
            _indexesByLicense.Add(driver.License, _drivers.Count - 1);
        }
        
        Log(_indexesByLicense.PrintDebugInfo());
        return true;
    }
}