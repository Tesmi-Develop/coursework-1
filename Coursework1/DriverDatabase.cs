using System;
using System.Collections.Generic;
using Coursework1.Data;

namespace Coursework1;

public class DriverDatabase
{
    public event Action<string>? LogMessage;

    private HashTable<DriverLicense, Driver> _drivers = new();

    private void Log(string message)
    {
        LogMessage?.Invoke(message);
    }

    public bool TrySetSettings(int capacity, int step)
    {
        if (_drivers.Count > 0)
            return false;

        _drivers = new HashTable<DriverLicense, Driver>(capacity, step);
        Log($"[INIT] Успещно применены настройки для ХТ: capacity = {capacity}, step = {step}");
        return true;
    }

    public bool TryAdd(Driver driver)
    {
        Log($"[ADD] Попытка добавления: {driver.License}");
        
        if (_drivers.ContainsKey(driver.License))
        {
            Log($"[FAIL] Ключ {driver.License} уже существует.");
            return false;
        }

        var success = _drivers.Add(driver.License, driver);
        
        if (success)
        {
            Log($"[SUCCESS] Добавлен: {driver.License}");
            Log(_drivers.PrintDebugInfo());
        }
        else
            Log("[ERROR] Ошибка добавления (таблица полна или конфликт).");

        return success;
    }

    public bool Remove(DriverLicense license, out Driver removedDriver)
    {
        Log($"[REMOVE] Попытка удаления: {license}");
        
        var success = _drivers.Remove(license, out removedDriver);

        if (success)
        {
            Log($"[SUCCESS] Удален: {removedDriver.License}");
            Log(_drivers.PrintDebugInfo());
        }
        else
            Log($"[FAIL] Ключ {license} не найден.");

        return success;
    }

    public bool TryFind(DriverLicense license, out Driver driver)
    {
        Log($"[FIND] Поиск: {license}");
        
        var found = _drivers.TryGetValue(license, out driver);

        if (found)
            Log($"[FOUND] Найден: {driver.Name}");
        else
            Log("[NOT FOUND] Не найден.");

        return found;
    }

    public IEnumerable<Driver> GetAllDrivers()
    {
        return _drivers.Values;
    }

    public int Count => _drivers.Count;
}