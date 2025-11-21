using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using Microsoft.Win32;

namespace PsdExtensions.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string PsdExtensionsGuid = "{5f4d0838-ea2a-40af-828f-c24bf8e0d90e}";
    private const string PsdExtensionsDll = "PsdExtensions.Plugin.dll";
    private static readonly string PsdExtensionsDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PsdExtensionsDll);

    public bool IsPsdExtensionsRegistered
    {
        get => (bool)GetValue(IsPsdExtensionsRegisteredProperty);
        set => SetValue(IsPsdExtensionsRegisteredProperty, value);
    }

    public static readonly DependencyProperty IsPsdExtensionsRegisteredProperty =
        DependencyProperty.Register(nameof(IsPsdExtensionsRegistered), typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

    public MainWindow()
    {
        DataContext = this;
        InitializeComponent();
        UpdatePsdExtensionsRegisterStatus();
    }

    private void UpdatePsdExtensionsRegisterStatus()
    {
        IsPsdExtensionsRegistered = DetectPsdExtensionsRegistered();

        static bool DetectPsdExtensionsRegistered()
        {
            using RegistryKey? clsid = Registry.ClassesRoot.OpenSubKey("CLSID");
            if (clsid != null)
            {
                using RegistryKey? psdExtensionsKey = clsid.OpenSubKey(PsdExtensionsGuid);
                if (psdExtensionsKey != null)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private async void OnInstallButtonClicked(object sender, RoutedEventArgs e)
    {
        await RegisterPsdExtensions();

        UpdatePsdExtensionsRegisterStatus();
    }

    private async void OnUninstallButtonClicked(object sender, RoutedEventArgs e)
    {
        await UnregisterPsdExtensions();

        UpdatePsdExtensionsRegisterStatus();
    }

    private static async Task RegisterPsdExtensions()
    {
        await CallRegSvr32();
    }

    private static async Task UnregisterPsdExtensions()
    {
        await CallRegSvr32(true);
    }

    private static async Task CallRegSvr32(bool isUnregister = false)
    {
        string argument = isUnregister
            ? $"{PsdExtensionsDllPath} /u"
            : PsdExtensionsDllPath;

        Process regsvr32 = new();
        ProcessStartInfo info = new()
        {
            Verb = "runas",
            Arguments = $"{argument} /s",
            UseShellExecute = true,
            FileName = "regsvr32.exe"
        };

        regsvr32.StartInfo = info;

        try
        {
            regsvr32.Start();
            await regsvr32.WaitForExitAsync();
            int exitCode = regsvr32.ExitCode;

            switch (exitCode)
            {
                case 5:
                    DisplayRunAsAdmin();
                    break;
                case 0:
                    break;
                default:
                    DisplayRegSvr32Error(exitCode);
                    break;
            }
        }
        catch (Win32Exception win32) when (win32.NativeErrorCode == 1223) // 用户取消了操作。
        {
            // ;-)
        }
        catch (Exception ex)
        {
            DisplayError(ex);
        }
    }

    private static void DisplayError(Exception ex)
    {
        MessageBox.Show($"出现错误：\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private static void DisplayRunAsAdmin()
    {
        MessageBox.Show($"此操作需要管理员权限，请以管理员身份运行此程序", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private static void DisplayRegSvr32Error(int errorCode)
    {
        MessageBox.Show($"安装失败：\n在使用 regsvr32 注册组件时出错，错误代码：{errorCode}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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