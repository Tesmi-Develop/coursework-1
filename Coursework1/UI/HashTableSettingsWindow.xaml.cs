using System.Windows;

namespace Coursework1.UI;

public partial class HashTableSettingsWindow : Window
{
    public int Capacity { get; private set; } = 11;
    public int Step { get; private set; } = 3;
    public bool IsSuccess { get; private set; }

    public HashTableSettingsWindow()
    {
        InitializeComponent();
        CapacityBox.Text = Capacity.ToString();
        StepBox.Text = Step.ToString();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(CapacityBox.Text, out var capacity) || capacity <= 0)
        {
            ErrorWindow.Show(this, "Размер таблицы должен быть положительным числом.");
            return;
        }

        if (!int.TryParse(StepBox.Text, out var step) || step <= 0)
        {
            ErrorWindow.Show(this, "Шаг должен быть положительным числом.");
            return;
        }

        if (step >= capacity)
        {
            ErrorWindow.Show(this, "Шаг (K) должен быть меньше размера таблицы.");
            return;
        }
        
        if (!IsPrime(capacity))
        {
            ErrorWindow.Show(this,"Размер таблицы должен быть простым числом для лучшего распределения.");
            return;
        }

        Capacity = capacity;
        Step = step;
        IsSuccess = true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        IsSuccess = false;
        DialogResult = false;
    }

    private bool IsPrime(int n)
    {
        if (n <= 1) return false;
        if (n <= 3) return true;
        if (n % 2 == 0 || n % 3 == 0) return false;
        for (var i = 5; i * i <= n; i += 6)
            if (n % i == 0 || n % (i + 2) == 0) return false;
        return true;
    }
}