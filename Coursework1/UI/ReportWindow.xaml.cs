using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

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
        if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
        {
            MessageBox.Show("Дата 'От' не может быть больше даты 'До'", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        decimal minAmount = 0;
        if (!string.IsNullOrWhiteSpace(MinAmountBox.Text))
        {
            if (!decimal.TryParse(MinAmountBox.Text, out minAmount))
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        
        Result = new ReportCriteria
        {
            FullName = FullNameBox.Text.Trim(),
            MinAmount = minAmount,
            From = StartDatePicker.SelectedDate ?? DateTime.MinValue,
            To = EndDatePicker.SelectedDate ?? DateTime.MaxValue
        };

        DialogResult = true;
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
    }
}

public class ReportCriteria
{
    public string FullName { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}