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

        DateBox.Text = new FormattedDate(DateTime.Now).ToString();
    }
    
    private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        var picker = sender as DatePicker;
        if (picker?.Template.FindName("PART_TextBox", picker) is DatePickerTextBox textBox && picker?.SelectedDate != null)
            textBox.Text = ((FormattedDate)picker.SelectedDate.Value).ToString();
    }

    private void GenerateReport_Click(object sender, RoutedEventArgs e)
    {
        if (!FormattedDate.TryParse(DateBox.Text, out var date, out var error))
        {
            ErrorWindow.Show(this, error);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(MinAmountBox.Text) || !int.TryParse(MinAmountBox.Text, out var amount) || amount < 0)
        {
            ErrorWindow.Show(this, "Введите корректную сумму");
            return;
        }

        if (!Parsers.TryParseFullName(LastNameBox.Text, FirstNameBox.Text, MiddleNameBox.Text, out var fullName,
                out error))
        {
            ErrorWindow.Show(this, error);
            return;
        }
        
        Result = new ReportCriteria
        {
            FullName = fullName,
            Amount = amount,
            Date = date,
        };

        DialogResult = true;
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }
}