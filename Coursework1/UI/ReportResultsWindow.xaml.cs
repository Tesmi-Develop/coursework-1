using System.Collections.ObjectModel;
using System.Windows;
using Coursework1.Data;

namespace Coursework1.UI;

public partial class ReportResultsWindow : Window
{
    public ObservableCollection<ReportItem> ReportItems { get; set; }
    
    public ReportResultsWindow(ReportItem[] items)
    {
        InitializeComponent();
        ReportItems = new ObservableCollection<ReportItem>(items);
        DataContext = this;
    }
}