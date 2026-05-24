using System.Collections.ObjectModel;
using System.Windows;
using Coursework1.Data;

namespace Coursework1;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<Driver> Drivers { get; set; } = [];
    public ObservableCollection<Fine> Fines { get; set; } = [];
    
    private readonly DebugWindow _debugWindow = new();
    
    public MainWindow()
    {
        DataContext = this;
        InitializeComponent();

        var license = new DriverLicense(999, 123);
        
        Drivers.Add(
            new Driver(
                license, 
                new FullName("Иванов", "Иван", "Иванович"), 
                VehicleCategory.B | VehicleCategory.A)
            );

        Fines.Add(new Fine(license, "Превышение максимальной скорости", 750, DateTime.Now));
        _debugWindow.Log("Интерфейс инициализирован");
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

        if (addWindow.ShowDialog() != true)
            return;

        if (!addWindow.IsSuccess)
        {
            ErrorWindow.Show(this, addWindow.ErrorMessage);
            return;
        }
        
        AddDriver(addWindow.CreatedDriver!.Value);
    }

    private bool ValidateDriverLicense(DriverLicense license)
    {
        return true;
    }

    private void AddDriver(Driver driver)
    {
        return;
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

        if (addWindow.ShowDialog() != true)
            return;

        if (!addWindow.IsSuccess)
        {
            ErrorWindow.Show(this, addWindow.ErrorMessage);
            return;
        }
        
        AddFine(addWindow.CreatedFine!.Value);
    }
}