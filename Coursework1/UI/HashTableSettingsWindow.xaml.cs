using System.Windows;

namespace Coursework1.UI;

public partial class HashTableSettingsWindow : Window
{
    public int Capacity { get; private set; } = 11;
    public bool IsSuccess { get; private set; }

    public HashTableSettingsWindow()
    {
        InitializeComponent();
        CapacityBox.Text = Capacity.ToString();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(CapacityBox.Text, out var capacity) || capacity <= 0)
        {
            ErrorWindow.Show(this, "Размер таблицы должен быть положительным числом.");
            return;
        }

        Capacity = capacity;
        IsSuccess = true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        IsSuccess = false;
        DialogResult = false;
    }
}