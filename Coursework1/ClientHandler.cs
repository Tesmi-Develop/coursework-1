using Coursework1.Data;

namespace Coursework1;

public class ClientHandler
{
    public event Action<Driver>? DriverAdded;
    public event Action<Driver>? DriverRemoved;
    public event Action<Driver[]>? FullUpdateDrivers;
    public event Action? ClearDrivers;

    public event Action<FineWithId>? FineAdded;
    public event Action<FineWithId, FineWithId?, int?>? FineRemoved; // removed, replacer, indexReplaced
    public event Action<FineWithId[]>? FullUpdateFines;
    public event Action? ClearFines;
    
    public event Action<string>? LogMessage;
    
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
        _driverDatabase.LogMessage += LogMessage;
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

    public bool TrySetHashTableSettings(int capacity, int step)
    {
        return _driverDatabase.TrySetSettings(capacity,  step);
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

    public void RemoveDriver(DriverLicense license)
    {
        if (!_driverDatabase.Remove(license, out var driver))
            return;
        
        DriverRemoved?.Invoke(driver);
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
        foreach (var fine in fines)
            RemoveFine(fine);
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
        if (license == DriverLicense.Invalid || !_driverDatabase.TryFind(license, out var foundDriver))
            return;

        DriverAdded?.Invoke(foundDriver);
    }
    
    public void DisableSearchInDrivers()
    {
        _isEnabledSearchInDrivers = false;
        SyncDrivers();
    }
    
    public void EnableSearchInFines(DriverLicense license)
    {
        _isEnabledSearchInFines = true;
        _filterInFines = license;
        ClearFines?.Invoke();
        if (license == DriverLicense.Invalid || !_driverDatabase.TryFind(license, out _))
            return;

        var result = _fineDatabase.Search(license);

        foreach (var fine in result)
            FineAdded?.Invoke(fine);
    }
    
    public void DisableSearchInFines()
    {
        _isEnabledSearchInFines = false;
        _filterInFines = null;
        SyncFines();
    }
}