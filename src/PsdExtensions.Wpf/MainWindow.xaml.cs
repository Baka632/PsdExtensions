using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using PsdExtensions.Wpf.ViewModels;

namespace PsdExtensions.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void OnNavigateGitHubLinkClicked(object sender, RoutedEventArgs e)
    {
        Hyperlink link = (Hyperlink)sender;
        ProcessStartInfo startInfo = new(link.NavigateUri.AbsoluteUri)
        {
            UseShellExecute = true
        };
        Process.Start(startInfo);
    }
}