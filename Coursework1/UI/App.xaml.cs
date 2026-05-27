using System.Windows;

namespace Coursework1.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Создаем базу данных один раз
        var driverDatabase = new DriverDatabase();
        var clientHandler = new ClientHandler(driverDatabase);
        
        var mainWindow = new MainWindow(clientHandler);
        mainWindow.Show();
    }
}