using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Coursework1.Data;

namespace Coursework1;

public delegate bool FinePredicate<in T>(T obj, out string error) where T : allows ref struct;

public class FineDatabase : IEnumerable<Fine>
{
    private readonly RedBlackTree<DateTime, int> _indexesByDateTime = new();
    private readonly RedBlackTree<DriverLicense, int> _indexesByDriverLicenses = new();
    private readonly DynamicArray<Fine> _fines = [];

    public int Count => _fines.Count;
    
    public FineWithId Add(Fine fine)
    {
        _fines.Add(fine);
        _indexesByDateTime.Insert(fine.Date, _fines.Count - 1);
        _indexesByDriverLicenses.Insert(fine.License, _fines.Count - 1);

        return new FineWithId { Fine = fine, Id = _fines.Count - 1 };
    }
    
    public Fine[] CreateReport(ReportCriteria reportCriteria, Predicate<Fine> predicate)
    {
        var indexes = _indexesByDateTime.GetValuesInRange(reportCriteria.From, reportCriteria.To, (index) => predicate(_fines[index]));
        var result = new Fine[indexes.Length];
        
        var currentIndex = 0;
        foreach (var index in indexes)
        {
            result[currentIndex] = _fines[index];
            currentIndex++;
        }
        
        return result;
    }

    public FineWithId[] Search(DriverLicense license)
    {
        if (!_indexesByDriverLicenses.TryFind(license, out var indexes))
            return [];

        var result = new FineWithId[indexes.Count];
        int currentIndex = 0;
        
        foreach (var index in indexes)
        {
            result[currentIndex] = new FineWithId { Id = index, Fine = _fines[index] };
            currentIndex++;
        }

        return result;
    }

    public bool TryRemove(int index, 
        out FineWithId removed,
        out FineWithId? replacer)
    {
        removed = default;
        replacer = null;
        
        if (index < 0 || index >= _fines.Count)
            return false;
        
        var record = _fines[index];
        
        if (!_indexesByDateTime.TryRemove(record.Date, index, out _))
            return false;

        _indexesByDriverLicenses.TryRemove(record.License, index, out _);
        
        var lastIndex = _fines.Count - 1;
        removed = new FineWithId { Id = lastIndex, Fine = _fines[lastIndex] };

        if (index != lastIndex)
        {
            var lastRecord =  _fines[lastIndex];
            
            _indexesByDateTime.Replace(lastRecord.Date, lastIndex, index);
            _indexesByDriverLicenses.Replace(lastRecord.License, lastIndex, index);
            _fines[index] = lastRecord;
            replacer = new FineWithId { Id = index, Fine = _fines[index] };
        }

        _fines.RemoveAt(lastIndex);
        return true;
    }

    public IEnumerator<Fine> GetEnumerator()
    {
        return _fines.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public bool TryImport(string filePath, FinePredicate<Fine> validateFine, out string error)
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
        
        Fine[] fines;

        try
        {
            fines = JsonSerializer.Deserialize<Fine[]>(fileContent) ?? [];
        }
        catch (JsonException e)
        {
            error = $"Ошибка при десериализации JSON: строка: {e.LineNumber + 1}, позиция: {e.BytePositionInLine}: {e.Message}";
            return false;
        }

        foreach (var fine in fines)
        {
            if (validateFine(fine, out error))
                continue;

            return false;
        }

        foreach (var fine in fines)
            Add(fine);
        
        return true;
    }
    
    public bool TryExport(string filePath, out string error)
    {
        error = string.Empty;
        string output;
        var drivers = new Fine[_fines.Count];
        var i = 0;

        foreach (var val in _fines)
            drivers[i++] = val;
        
        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        
        try
        {
            output = JsonSerializer.Serialize(drivers, options);
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
}