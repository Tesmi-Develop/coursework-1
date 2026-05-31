using System.Collections;
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

    public event Action<string>? LogMessage;

    private void Log(string message) => LogMessage?.Invoke(message);

    public int Count => _fines.Count;
    
    public FineWithId Add(Fine fine)
    {
        Log($"[ADD] {fine.License} | {fine.Date}");
        
        _fines.Add(fine);
        var newIndex = _fines.Count - 1;
        
        _indexesByDateTime.Insert(fine.Date, newIndex);
        _indexesByDriverLicenses.Insert(fine.License, newIndex);

        Log($"[SUCCESS] ID: {newIndex}");
        Log(_indexesByDriverLicenses.ToVisualString());
        return new FineWithId { Fine = fine, Id = newIndex };
    }
    
    public Fine[] CreateReport(ReportCriteria reportCriteria, Predicate<Fine> predicate)
    {
        Log($"[REPORT] {reportCriteria.From} - {reportCriteria.To}");
        
        var indexes = _indexesByDateTime.GetValuesInRange(
            reportCriteria.From, 
            reportCriteria.To, 
            index => predicate(_fines[index]));
        
        Log($"[REPORT] Found: {indexes.Length}");
        
        var result = new Fine[indexes.Length];
        for (var i = 0; i < indexes.Length; i++)
        {
            result[i] = _fines[indexes[i]];
        }
        
        return result;
    }

    public FineWithId[] Search(DriverLicense license, out int steps)
    {
        Log($"[SEARCH] {license}");
        
        if (!_indexesByDriverLicenses.TryFind(license, out var indexes, out steps))
        {
            Log($"[NOT FOUND] {license}");
            return [];
        }

        var result = new FineWithId[indexes.Count];
        var current = 0;
        foreach (var index in indexes)
        {
            result[current++] = new FineWithId { Id = index, Fine = _fines[index] };
        }

        Log($"[FOUND] Count: {result.Length}");
        return result;
    }
    
    public bool HasLicense(DriverLicense license)
    {
        return _indexesByDriverLicenses.TryFind(license, out var indexes) && !indexes.Empty;
    }

    public bool TryRemove(int index, out FineWithId removed, out FineWithId? replacer)
    {
        removed = default;
        replacer = null;
        
        Log($"[REMOVE] ID: {index}");
        
        if (index < 0 || index >= _fines.Count)
        {
            Log($"[FAIL] Out of range: {index}");
            return false;
        }
        
        var record = _fines[index];
        
        if (!_indexesByDateTime.TryRemove(record.Date, index, out _))
        {
            Log($"[ERROR] Index sync failed: {index}");
            return false;
        }

        _indexesByDriverLicenses.TryRemove(record.License, index, out _);
        
        var lastIndex = _fines.Count - 1;
        var lastRecord = _fines[lastIndex];
        removed = new FineWithId { Id = lastIndex, Fine = lastRecord };

        if (index != lastIndex)
        {
            Log($"[SWAP] {lastIndex} -> {index}");
            
            _indexesByDateTime.Replace(lastRecord.Date, lastIndex, index);
            _indexesByDriverLicenses.Replace(lastRecord.License, lastIndex, index);
            _fines[index] = lastRecord;
            replacer = new FineWithId { Id = index, Fine = _fines[index] };
        }

        _fines.RemoveAt(lastIndex);
        Log($"[SUCCESS] Remaining: {_fines.Count}");
        Log(_indexesByDriverLicenses.ToVisualString());
        return true;
    }

    public bool TryImport(string filePath, FinePredicate<Fine> validateFine, out string error)
    {
        Log($"[IMPORT] {filePath}");
        error = string.Empty;

        if (!File.Exists(filePath))
        {
            error = "File not found";
            Log($"[FAIL] {error}");
            return false;
        }

        try
        {
            var content = File.ReadAllText(filePath);
            var fines = JsonSerializer.Deserialize<Fine[]>(content);

            if (fines == null) 
                return true;

            Log($"[IMPORT] Validating {fines.Length} items...");

            foreach (var fine in fines)
            {
                if (validateFine(fine, out error)) 
                    continue;
                Log($"[VALIDATION FAIL] {fine.License}: {error}");
                return false;
            }

            foreach (var fine in fines) Add(fine);
            
            Log("[SUCCESS] Import completed");
            Log(_indexesByDriverLicenses.ToVisualString());
            return true;
        }
        catch (JsonException e)
        {
            error = $"Ошибка при десериализации JSON: строка: {e.LineNumber + 1}, позиция: {e.BytePositionInLine}: {e.Message}";
            Log($"[ERROR] {error}");
            return false;
        }
    }
    
    public bool TryExport(string filePath, out string error)
    {
        Log($"[EXPORT] {filePath}");
        error = string.Empty;
        
        try
        {
            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            
            var data = new Fine[_fines.Count];
            for (var i = 0; i < _fines.Count; i++) data[i] = _fines[i];

            File.WriteAllText(filePath, JsonSerializer.Serialize(data, options));
            
            Log($"[SUCCESS] Exported: {data.Length}");
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            Log($"[ERROR] {e.Message}");
            return false;
        }
    }

    public IEnumerator<Fine> GetEnumerator() => _fines.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}