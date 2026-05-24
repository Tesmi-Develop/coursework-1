using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Coursework1;

public partial class ReportWindow : Window
{
    // Результат работы окна
    public ReportCriteria? Result { get; private set; }

    public ReportWindow()
    {
        InitializeComponent();
        
        StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
        EndDatePicker.SelectedDate = DateTime.Now;
    }

    private void GenerateReport_Click(object sender, RoutedEventArgs e)
    {
        // 1. Проверка дат
        if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
        {
            MessageBox.Show("Дата 'От' не может быть больше даты 'До'", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 2. Парсинг суммы (если введена)
        decimal minAmount = 0;
        if (!string.IsNullOrWhiteSpace(MinAmountBox.Text))
        {
            if (!decimal.TryParse(MinAmountBox.Text, out minAmount))
            {
                MessageBox.Show("Введите корректную сумму", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        // 3. Формируем результат
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

// Модель критериев отчета
public class ReportCriteria
{
    public string FullName { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}