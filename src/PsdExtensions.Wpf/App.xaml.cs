using System.Windows;

namespace PsdExtensions.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
#pragma warning disable WPF0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。
        // Win11 => Fluent, other => Aero2
        ThemeMode = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000, 0)
            ? ThemeMode.System
            : ThemeMode.None;
#pragma warning restore WPF000
    }
}
