using Coursework1.Data;
using Coursework1.Databases;

namespace Coursework1.Client;

public class ClientHandler
{
    public event Action<Driver>? DriverAdded;
    public event Action<Driver, Driver?>? DriverRemoved; // removed, replacer
    public event Action<Driver[]>? FullUpdateDrivers;
    public event Action? ClearDrivers;

    public event Action<FineWithId>? FineAdded;
    public event Action<FineWithId, FineWithId?, int?>? FineRemoved; // removed, replacer, indexReplaced
    public event Action<FineWithId[]>? FullUpdateFines;
    public event Action? ClearFines;
    
    public event Action<string>? DriversLogMessage;
    public event Action<string>? FinesLogMessage;

    public event Action<string>? ChangedStatus; 
    
    private readonly DriverDatabase _driverDatabase;
    private readonly FineDatabase _fineDatabase;
    
    private bool _isEnabledSearchInDrivers;
    private bool _isEnabledSearchInFines = false;

    private DriverLicense? _filterInFines;

    public ClientHandler(DriverDatabase driverDatabase, FineDatabase fineDatabase)
    {
        _driverDatabase = driverDatabase;
        _fineDatabase = fineDatabase;
    }

    public void Start()
    {
        _driverDatabase.LogMessage += DriversLogMessage;
        _fineDatabase.LogMessage += FinesLogMessage;
        SyncDrivers();
        SyncFines();
    }

    public bool HasDriver(DriverLicense license)
    {
        return _driverDatabase.Has(license);
    }

    private void SyncDrivers()
    {
        var drivers = new Driver[_driverDatabase.Count];
        var i = 0;
        
        foreach (var driver in _driverDatabase.GetAllDrivers())
        {
            drivers[i] = driver;
            i++;
        }
        
        FullUpdateDrivers?.Invoke(drivers);
    }
    
    private void SyncFines()
    {
        var fines = new FineWithId[_fineDatabase.Count];
        var i = 0;

        foreach (var fine in _fineDatabase)
        {
            fines[i] = new FineWithId { Id = i, Fine = fine };
            i++;
        }
        
        FullUpdateFines?.Invoke(fines);
    }
    
    public void AddDrivers(Driver[] drivers)
    {
        foreach (var driver in drivers)
            AddDriver(driver);
    }

    public IEnumerable<Driver> GetAllDrivers()
    {
        return _driverDatabase.GetAllDrivers();
    }

    public bool TrySetHashTableSettings(int capacity)
    {
        return _driverDatabase.TrySetSettings(capacity);
    }
    
    public bool AddDriver(Driver driver)
    {
        if (!_driverDatabase.TryAdd(driver))
            return false;
        
        if (!_isEnabledSearchInDrivers)
            DriverAdded?.Invoke(driver);

        return true;
    }
    
    public void RemoveDrivers(Driver[] drivers)
    {
        foreach (var driver in drivers)
            RemoveDriver(driver.License);
    }

    public bool HasFine(DriverLicense license)
    {
        return _fineDatabase.HasLicense(license);
    }

    public void RemoveDriver(DriverLicense license)
    {
        if (_fineDatabase.HasLicense(license) || !_driverDatabase.Remove(license, out var driver, out var replacer))
            return;

        if (_isEnabledSearchInDrivers)
            replacer = null;
        
        DriverRemoved?.Invoke(driver, replacer);
    }

    public bool AddFine(Fine fine)
    {
        if (!_driverDatabase.Has(fine.License))
            return false;
        
        var result = _fineDatabase.Add(fine);
        if (!_isEnabledSearchInFines)
        {
            FineAdded?.Invoke(result);
            return true;
        }

        if (fine.License == _filterInFines!.Value)
            FineAdded?.Invoke(result);
        
        return true;
    }

    public void RemoveFines(FineWithId[] fines)
    {
        for (var i = fines.Length - 1; i >= 0; i--)
            RemoveFine(fines[i]);
    }

    public void RemoveFine(FineWithId fine)
    {
        if (!_fineDatabase.TryRemove(fine.Id, out var removed, out var replacer))
            return;

        var indexRemoved = replacer.HasValue ? replacer.Value.Id : (int?)null;
        if (_isEnabledSearchInFines)
        {
            if (replacer.HasValue && replacer.Value.Fine.License != _filterInFines!.Value)
                replacer = null;
        }
        
        FineRemoved?.Invoke(removed, replacer, _isEnabledSearchInFines ? indexRemoved : null);
    }

    public void EnableSearchInDrivers(DriverLicense license)
    {
        _isEnabledSearchInDrivers = true;
        ClearDrivers?.Invoke();
        if (license == DriverLicense.Invalid || !_driverDatabase.TryFind(license, out var foundDriver, out var steps))
            return;

        ChangedStatus?.Invoke($"Поиск водителя | Шагов поиска: {steps}");
        DriverAdded?.Invoke(foundDriver);
    }
    
    public void DisableSearchInDrivers()
    {
        _isEnabledSearchInDrivers = false;
        
        if (!_isEnabledSearchInFines)
            ChangedStatus?.Invoke(string.Empty);
        
        SyncDrivers();
    }
    
    public void EnableSearchInFines(DriverLicense license)
    {
        _isEnabledSearchInFines = true;
        _filterInFines = license;
        ClearFines?.Invoke();
        if (license == DriverLicense.Invalid)
            return;

        var result = _fineDatabase.Search(license, out var steps);

        ChangedStatus?.Invoke($"Поиск штрафов | Шагов поиска: {steps}");
        foreach (var fine in result)
            FineAdded?.Invoke(fine);
    }
    
    public void DisableSearchInFines()
    {
        _isEnabledSearchInFines = false;
        _filterInFines = null;
        
        if (!_isEnabledSearchInDrivers)
            ChangedStatus?.Invoke(string.Empty);
        
        SyncFines();
    }

    public ReportItem[] CreateReport(ReportCriteria reportCriteria)
    {
        var fines = _fineDatabase.CreateReport(reportCriteria, (fine) =>
        {
            if (!_driverDatabase.TryFind(fine.License, out var driver))
                throw new Exception("Driver not found");

            return reportCriteria.FullName == driver.FullName && reportCriteria.Amount <= fine.Price;
        });
        
        var result = new ReportItem[fines.Length];
        var i = 0;
        foreach (var fine in fines)
        {
            if (!_driverDatabase.TryFind(fine.License, out var driver))
                throw new Exception("Driver not found");
            
            result[i] = new ReportItem
            {
                License = fine.License,
                FullName = driver.FullName,
                Categories = driver.Categories,
                Article = fine.Article,
                Price = fine.Price,
                Date = fine.Date
            };
            i++;
        }

        return result;
    }

    public bool TryImportDrivers(string filePath, out string error)
    {
        if (!_driverDatabase.TryImport(filePath, out error))
            return false;
        
        DisableSearchInDrivers();
        ClearDrivers?.Invoke();
        SyncDrivers();
        return true;
    }
    
    public bool TryExportDrivers(string filePath, out string error)
    {
        return _driverDatabase.TryExport(filePath, out error);
    }
    
    public bool TryImportFines(string filePath, out string error)
    {
        if (!_fineDatabase.TryImport(filePath, (fine, out error) =>
            {
                if (!_driverDatabase.Has(fine.License))
                {
                    error = $"ВУ: {fine.License} отсутствует в базе";
                    return false;
                }

                error = string.Empty;
                return true;
            }, out error))
            return false;
        
        DisableSearchInFines();
        ClearFines?.Invoke();
        SyncFines();
        return true;
    }
    
    public bool TryExportFines(string filePath, out string error)
    {
        return _fineDatabase.TryExport(filePath, out error);
    }
}