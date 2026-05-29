using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Coursework1.Data;
using Coursework1.Utilities;

namespace Coursework1.UI;

public partial class AddRecordWindow : Window
{
    public VehicleCategory[] AllCategories { get; }
    public Driver? CreatedDriver { get; private set; }
    public bool IsSuccess => CreatedDriver is not null;
    private VehicleCategory _selectedCategories = VehicleCategory.None;
    
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
    
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Parsers.TryParseFullName(LastNameBox.Text, FirstNameBox.Text, MiddleNameBox.Text, out var fullName, out var error ) || 
            !Parsers.TryParseLicense(VuBox.Text, out var driverLicense, out error))
        {
            ErrorWindow.Show(this, error);
            return;
        }

        if (_selectedCategories == VehicleCategory.None)
        {
            ErrorWindow.Show(this, "Необходимо выбрать хотя бы одну категорию транспортного средства");
            return;
        }
        
        CreatedDriver = new Driver
        {
            License = driverLicense,
            FullName = new FullName(LastNameBox.Text, FirstNameBox.Text, MiddleNameBox.Text),
            Categories = _selectedCategories
        };
        DialogResult = true;
    }
}