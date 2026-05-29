using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Coursework1.Data;

namespace Coursework1;

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
        var currentIndex = 0;
        
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
}