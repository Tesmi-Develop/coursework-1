using System.Windows;

namespace Coursework1;

public partial class ErrorWindow : Window
{
    public ErrorWindow(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
    }
    
    public static void Show(Window owner, string message)
    {
        var win = new ErrorWindow(message) { Owner = owner };
        win.ShowDialog();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}