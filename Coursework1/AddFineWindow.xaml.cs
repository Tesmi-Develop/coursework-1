using System.Text.RegularExpressions;
using System.Windows;
using Coursework1.Data;
using Coursework1.Utilities;

namespace Coursework1;

public partial class AddFineWindow : Window
{
    private readonly Func<DriverLicense, bool> _validateDriverLicense;
    public Fine? CreatedFine { get; private set; }
    public DateTime CurrentDate { get; } = DateTime.Now;
    public string ErrorMessage { get; private set; } = string.Empty;
    public bool IsSuccess => CreatedFine is not null;

    public AddFineWindow(Func<DriverLicense, bool> validateDriverLicense)
    {
        _validateDriverLicense = validateDriverLicense;
        InitializeComponent();
        DataContext = this;
        ViolationDatePicker.SelectedDate = DateTime.Now;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViolationTitleBox.Text))
        {
            ErrorWindow.Show(this, "Введите описание нарушения");
            return;
        }
        
        if (!ViolationDatePicker.SelectedDate.HasValue)
        {
            ErrorWindow.Show(this, "Выберите дату нарушения");
            return;
        }
        
        if (!int.TryParse(AmountBox.Text, out var amount) || amount <= 0)
        {
            ErrorWindow.Show(this, "Введите корректную сумму штрафа");
            return;
        }

        if (!DriverLicenseParser.TryParse(VuBox.Text, out var license, out var error))
        {
            ErrorWindow.Show(this, error);
            return;
        }

        if (!_validateDriverLicense(license))
        {
            ErrorWindow.Show(this, "Данного водителя не существует в базе");
            return;
        }
        
        CreatedFine = new Fine(license, ViolationTitleBox.Text, amount, ViolationDatePicker.SelectedDate.Value);
        DialogResult = true;
    }
    
    private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }
}