using Coursework1.Data;

namespace Coursework1;

public class ClientHandler
{
    public event Action<Driver>? DriverAdded;
    public event Action<Driver>? DriverRemoved;
    public event Action<Driver[]>? FullUpdateDrivers;
    public event Action? ClearDrivers;
    public event Action<string>? LogMessage;
    private readonly DriverDatabase _driverDatabase;
    private bool _isEnabledSearchInDrivers = false;

    public ClientHandler(DriverDatabase driverDatabase)
    {
        _driverDatabase = driverDatabase;
    }

    public void Start()
    {
        _driverDatabase.LogMessage += LogMessage;
        SyncDrivers();
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

    public void RemoveDrivers(DriverLicense[] licenses)
    {
        foreach (var license in licenses)
        {
            RemoveDriver(license);
        }
    }
    
    public void RemoveDrivers(Driver[] drivers)
    {
        foreach (var driver in drivers)
        {
            RemoveDriver(driver.License);
        }
    }

    public void RemoveDriver(DriverLicense license)
    {
        if (!_driverDatabase.Remove(license, out var driver))
            return;
        
        DriverRemoved?.Invoke(driver);
    }

    public void EnableSearchInDrivers(DriverLicense license)
    {
        _isEnabledSearchInDrivers = true;
        ClearDrivers?.Invoke();
        if (license == DriverLicense.Invalid || !_driverDatabase.TryFind(license, out var foundDriver))
            return;

        DriverAdded.Invoke(foundDriver);
    }
    
    public void DisableSearchInDrivers()
    {
        _isEnabledSearchInDrivers = false;
        SyncDrivers();
    }
}