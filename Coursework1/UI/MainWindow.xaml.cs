using System.Collections.ObjectModel;
using System.Windows;
using Coursework1.Data;
using Coursework1.Utilities;

namespace Coursework1.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<Driver> Drivers { get; set; } = [];
    public ObservableCollection<Fine> Fines { get; set; } = [];
    private readonly ClientHandler _clientHandler;
    
    private readonly DebugWindow _debugWindow = new();
    
    public MainWindow(ClientHandler clientHandler)
    {
        _clientHandler = clientHandler;
        DataContext = this;
        InitializeComponent();
        InitEvents();
        _clientHandler.Start();

        _debugWindow.Log("Интерфейс инициализирован");
    }

    private void InitEvents()
    {
        _clientHandler.DriverAdded += driver =>
        {
            Drivers.Add(driver);
        };

        _clientHandler.DriverRemoved += driver =>
        {
            Drivers.Remove(driver);
        };

        _clientHandler.ClearDrivers += () =>
        {
            Drivers.Clear();
        };

        _clientHandler.FullUpdateDrivers += drivers =>
        {
            Drivers.Clear();
            foreach (var driver in drivers)
                Drivers.Add(driver);
        };

        _clientHandler.LogMessage += _debugWindow.Log;
    }
    
    private void DebugButton_Click(object sender, RoutedEventArgs e)
    {
        if (_debugWindow.IsVisible)
            _debugWindow.Activate();
        else
            _debugWindow.Show();
    }

    public void AddDriver_Click(object sender, RoutedEventArgs e)
    {
        var addWindow = new AddRecordWindow
        {
            Owner = this
        };

        if (addWindow.ShowDialog() != true || !addWindow.IsSuccess)
            return;
        
        AddDriver(addWindow.CreatedDriver!.Value);
    }

    private bool ValidateDriverLicense(DriverLicense license)
    {
        return true;
    }

    private bool AddDriver(Driver driver)
    {
        return _clientHandler.AddDriver(driver);
    }

    private void AddFine(Fine fine)
    {
        return;
    }
    
    public void AddFine_Click(object sender, RoutedEventArgs e)
    {
        var addWindow = new AddFineWindow(ValidateDriverLicense)
        {
            Owner = this
        };

        if (addWindow.ShowDialog() != true || !addWindow.IsSuccess)
            return;
        
        AddFine(addWindow.CreatedFine!.Value);
    }

    public void RemoveDrivers_Click(object sender, RoutedEventArgs e)
    {
        var selectedDrivers = DriversGrid.SelectedItems.Cast<Driver>().ToArray();
        if (selectedDrivers.Length <= 0)
            return;
        
        _clientHandler.RemoveDrivers(selectedDrivers);
    }

    public void FindDriver_Click(object sender, RoutedEventArgs e)
    {
        var input = SearchInDrivers.Text;
        if (string.IsNullOrEmpty(input))
        {
            _clientHandler.DisableSearchInDrivers();
            return;
        }

        if (!DriverLicenseParser.TryParse(input, out var output, out _))
            output = DriverLicense.Invalid;
        
        _clientHandler.EnableSearchInDrivers(output);
    }

    public void SetHashTableCapacity_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new HashTableSettingsWindow()
        {
            Owner = this
        };

        if (settingsWindow.ShowDialog() != true || !settingsWindow.IsSuccess)
            return;

        if (!_clientHandler.TrySetHashTableSettings(settingsWindow.Capacity, settingsWindow.Step))
        {
            ErrorWindow.Show(this,"Нельзя задать параметры ХТ когда есть записи");
        }
    }
}