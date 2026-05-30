using System.Collections.ObjectModel;
using System.Windows;
using Coursework1.Data;
using Coursework1.Utilities;
using Microsoft.Win32;

// ReSharper disable CollectionNeverQueried.Global

namespace Coursework1.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public ObservableCollection<Driver> Drivers { get; set; } = [];
    public ObservableCollection<FineWithId> Fines { get; set; } = [];
    
    private readonly ClientHandler _clientHandler;
    private readonly DebugWindow _driversDebugWindow = new();
    private readonly DebugWindow _finesDebugWindow = new();
    
    public MainWindow(ClientHandler clientHandler)
    {
        _clientHandler = clientHandler;
        DataContext = this;
        InitializeComponent();
        InitEvents();
        _clientHandler.Start();

        _driversDebugWindow.Title = "Откладка ХТ";
        _finesDebugWindow.Title = "Откладка КЧ дерева";
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

        _clientHandler.FineAdded += fine =>
        {
            Fines.Add(fine);
        };

        _clientHandler.FineRemoved += (removed, replacer, indexRemoved) =>
        {
            if (Fines.Count == 0)
                return;

            var idToRemove = -1;
            
            for (var i = 0; i < Fines.Count; i++)
            {
                if (Fines[i].Id == removed.Id)
                    idToRemove = i;
                
                if (replacer.HasValue && Fines[i].Id == replacer.Value.Id)
                    Fines[i] = replacer.Value;

                if (indexRemoved.HasValue && Fines[i].Id == indexRemoved)
                    idToRemove = i;
            }
            
            if (idToRemove != -1)
                Fines.RemoveAt(idToRemove);
        };

        _clientHandler.FullUpdateFines += fines =>
        {
            Fines.Clear();
            foreach (var fine in fines)
                Fines.Add(fine);
        };
        
        _clientHandler.ClearFines += () =>
        {
            Fines.Clear();
        };
        
        _clientHandler.DriversLogMessage += _driversDebugWindow.Log;
        _clientHandler.FinesLogMessage += _finesDebugWindow.Log;
    }
    
    private void DebugDrivers_Click(object sender, RoutedEventArgs e)
    {
        if (_driversDebugWindow.IsVisible)
            _driversDebugWindow.Activate();
        else
            _driversDebugWindow.Show();
    }
    
    private void DebugFines_Click(object sender, RoutedEventArgs e)
    {
        if (_finesDebugWindow.IsVisible)
            _finesDebugWindow.Activate();
        else
            _finesDebugWindow.Show();
    }

    public void AddDriver_Click(object sender, RoutedEventArgs e)
    {
        var addWindow = new AddRecordWindow((license) => !_clientHandler.HasDriver(license))
        {
            Owner = this
        };

        if (addWindow.ShowDialog() != true || !addWindow.IsSuccess)
            return;
        
        AddDriver(addWindow.CreatedDriver!.Value);
    }

    private bool ValidateDriverLicense(DriverLicense license)
    {
        return _clientHandler.HasDriver(license);
    }

    private void AddDriver(Driver driver)
    {
        _clientHandler.AddDriver(driver);
    }

    private void AddFine(Fine fine)
    {
        _clientHandler.AddFine(fine);
    }

    public void RemoveDrivers_Click(object sender, RoutedEventArgs e)
    {
        var selectedDrivers = DriversGrid.SelectedItems.Cast<Driver>().ToArray();
        if (selectedDrivers.Length <= 0)
            return;

        if (selectedDrivers.Length == 1 && _clientHandler.HasFine(selectedDrivers[0].License))
        {
            ErrorWindow.Show(this, "Нельзя удалить водителя пока на него есть штрафы");
            return;
        }
        
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

        if (!Parsers.TryParseLicense(input, out var output, out _))
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

    public void RemoveFines_Click(object sender, RoutedEventArgs e)
    {
        var selectedFines = FinesGrid.SelectedItems.Cast<FineWithId>().ToArray();
        if (selectedFines.Length <= 0)
            return;
        
        _clientHandler.RemoveFines(selectedFines);
    }

    public void FindFines_Click(object sender, RoutedEventArgs e)
    {
        var input = SearchInFines.Text;
        if (string.IsNullOrEmpty(input))
        {
            _clientHandler.DisableSearchInFines();
            return;
        }

        if (!Parsers.TryParseLicense(input, out var output, out _))
            output = DriverLicense.Invalid;
        
        _clientHandler.EnableSearchInFines(output);
    }

    public void ReportCreate_Click(object sender, RoutedEventArgs e)
    {
        var reportWindow = new ReportWindow()
        {
            Owner = this,
        };

        if (reportWindow.ShowDialog() != true || reportWindow.Result is null)
            return;

        var items = _clientHandler.CreateReport(reportWindow.Result);
        
        var reportResultWindow = new ReportResultsWindow(items)
        {
            Owner = this
        };
        
        reportResultWindow.Show();
    }

    public void ImportDrivers_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = "Выберите файл отчета"
        };
        
        if (openFileDialog.ShowDialog() != true)
            return;
        
        var selectedFilePath = openFileDialog.FileName;

        if (_clientHandler.TryImportDrivers(selectedFilePath, out var error))
            return;
        
        ErrorWindow.Show(this, error);
    }
    
    private void ExportDriversButton_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "JSON файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
            DefaultExt = "json",
            FileName = "report_drivers",
            Title = "Экспорт данных"
        };
        
        if (saveFileDialog.ShowDialog() != true)
            return;
        
        var filePath = saveFileDialog.FileName;

        if (!_clientHandler.TryExportDrivers(filePath, out var error))
        {
            ErrorWindow.Show(this, error);
            return;
        }
        
        MessageBox.Show("Файл успешно экспортирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    public void ImportFines_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = "Выберите файл отчета"
        };
        
        if (openFileDialog.ShowDialog() != true)
            return;
        
        var selectedFilePath = openFileDialog.FileName;

        if (_clientHandler.TryImportFines(selectedFilePath, out var error))
            return;
        
        ErrorWindow.Show(this, error);
    }
    
    private void ExportFinesButton_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Filter = "JSON файлы (*.json)|*.json|Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
            DefaultExt = "json",
            FileName = "report_fines",
            Title = "Экспорт данных"
        };
        
        if (saveFileDialog.ShowDialog() != true)
            return;
        
        var filePath = saveFileDialog.FileName;

        if (!_clientHandler.TryExportFines(filePath, out var error))
        {
            ErrorWindow.Show(this, error);
            return;
        }
        
        MessageBox.Show("Файл успешно экспортирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}