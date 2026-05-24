using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Coursework1.Data;
using Coursework1.Utilities;

namespace Coursework1;

public partial class AddRecordWindow : Window
{
    public VehicleCategory[] AllCategories { get; }
    public Driver? CreatedDriver { get; private set; }
    public bool IsSuccess => CreatedDriver is not null;
    private VehicleCategory _selectedCategories = VehicleCategory.None;
    public string ErrorMessage { get; private set; } = string.Empty;
    
    public AddRecordWindow()
    {
        AllCategories = Enum.GetValues<VehicleCategory>()
            .Where(c => c != VehicleCategory.None)
            .ToArray();

        InitializeComponent();
        
        DataContext = this;
    }
    
    private void CheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox { Tag: VehicleCategory category } cb) 
            return;
        
        if (cb.IsChecked == true)
            _selectedCategories |= category;
        else
            _selectedCategories &= ~category;
    }

    private bool TryParseLicense(string input, out DriverLicense driverLicense, out string error)
    {
        error = string.Empty;
        driverLicense = default;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Введите данные ВУ";
            return false;
        }

        return DriverLicenseParser.TryParse(input, out driverLicense, out error);
    }

    private bool TryParseFullName(string lastName, string firstName, string middleName, out FullName name, out string error)
    {
        error = string.Empty;
        name = default;
        
        const string pattern = "^[А-ЯЁ][а-яё]{1,}$";
        
        var isLastValid = Regex.IsMatch(lastName, pattern);
        var isFirstValid = Regex.IsMatch(firstName, pattern);
        var isMiddleValid = Regex.IsMatch(middleName, pattern);

        if (isLastValid && isFirstValid && isMiddleValid)
        {
            name = new FullName(lastName, firstName, middleName);
            return true;
        }

        error = "Некорректное ФИО";
        return false;
    }
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        
        if (!TryParseFullName(LastNameBox.Text, FirstNameBox.Text, MiddleNameBox.Text, out var fullName, out var error ) || 
            !TryParseLicense(VuBox.Text, out var driverLicense, out error))
        {
            ErrorMessage = error;
            return;
        }

        if (_selectedCategories == VehicleCategory.None)
        {
            ErrorMessage = "Необходимо выбрать хотя бы одну категорию транспортного средства";
            return;
        }
        
        CreatedDriver = new Driver
        {
            License = driverLicense,
            Name = new FullName(LastNameBox.Text, FirstNameBox.Text, MiddleNameBox.Text),
            Categories = _selectedCategories
        };
    }
}