using System.Text.RegularExpressions;
using System.Windows;
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
        
        StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
        EndDatePicker.SelectedDate = DateTime.Now;
    }

    private void GenerateReport_Click(object sender, RoutedEventArgs e)
    {
        if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
        {
            ErrorWindow.Show(this, "Даты не выбраны");
            return;
        }
        
        if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
        {
            ErrorWindow.Show(this, "Дата 'От' не может быть больше даты 'До'");
            return;
        }
        
        var amount = 0;
        if (!string.IsNullOrWhiteSpace(MinAmountBox.Text))
        {
            if (!int.TryParse(MinAmountBox.Text, out amount))
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
            From = StartDatePicker.SelectedDate!.Value,
            To = EndDatePicker.SelectedDate!.Value
        };

        DialogResult = true;
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }
}