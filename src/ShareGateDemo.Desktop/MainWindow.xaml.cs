using System.Windows;
using ShareGateDemo.Desktop.ViewModels;

namespace ShareGateDemo.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
