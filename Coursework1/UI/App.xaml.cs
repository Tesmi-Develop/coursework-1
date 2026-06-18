using System.Windows;
using Coursework1.Client;
using Coursework1.Collections;
using Coursework1.Databases;

namespace Coursework1.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var driverDatabase = new DriverDatabase();
        var fineDatabase = new FineDatabase();
        var clientHandler = new ClientHandler(driverDatabase, fineDatabase);
        
        var mainWindow = new MainWindow(clientHandler);
        mainWindow.Show();
    }
}