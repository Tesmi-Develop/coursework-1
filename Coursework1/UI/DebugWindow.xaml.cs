using System.Windows;

namespace Coursework1.UI;

public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();
    }

    public void Log(string message)
    {
        Dispatcher.Invoke(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogBox.AppendText($"[{timestamp}] {message}{Environment.NewLine}");

            if (AutoScrollCheck.IsChecked == true)
                LogBox.ScrollToEnd();
        });
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        LogBox.Clear();
    }
    
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}