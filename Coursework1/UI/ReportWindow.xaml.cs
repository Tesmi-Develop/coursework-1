using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Coursework1.Data;
using Coursework1.Utilities;

namespace Coursework1.UI;

public partial class ReportWindow
{
    public ReportCriteria? Result { get; private set; }

    public ReportWindow()
    {
        InitializeComponent();
        
        DatePicker.SelectedDate = DateTime.Now;
    }
    
    private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        var picker = sender as DatePicker;
        if (picker?.Template.FindName("PART_TextBox", picker) is DatePickerTextBox textBox && picker?.SelectedDate != null)
            textBox.Text = ((FormattedDate)picker.SelectedDate.Value).ToString();
    }

    private void GenerateReport_Click(object sender, RoutedEventArgs e)
    {
        if (!DatePicker.SelectedDate.HasValue)
        {
            ErrorWindow.Show(this, "Дата не выбрана");
            return;
        }
        
        var amount = 0;
        if (!string.IsNullOrWhiteSpace(MinAmountBox.Text))
        {
            if (!int.TryParse(MinAmountBox.Text, out amount) || amount < 0)
            {
                ErrorWindow.Show(this, "Введите корректную сумму");
                return;
            }
        }

        if (!Parsers.TryParseFullName(LastNameBox.Text, FirstNameBox.Text, MiddleNameBox.Text, out var fullName,
                out var error))
        {
            ErrorWindow.Show(this, error);
            return;
        }
        
        Result = new ReportCriteria
        {
            FullName = fullName,
            Amount = amount,
            Date = DatePicker.SelectedDate!.Value,
        };

        DialogResult = true;
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }
}