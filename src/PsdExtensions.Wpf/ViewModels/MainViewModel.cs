using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace PsdExtensions.Wpf.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isPsdExtensionsRegistered;
    [ObservableProperty]
    private bool isPsdExtensionsRequireReinstall;

    [ObservableProperty]
    private ICommand? psdExtensionActionButtonCommand;
    [ObservableProperty]
    private string psdExtensionActionButtonText = string.Empty;

    [ObservableProperty]
    private string psdExtensionStatusText = string.Empty;
    [ObservableProperty]
    private SolidColorBrush psdExtensionStatusTextBrush = SystemColors.ControlTextBrush;

    public MainViewModel()
    {
        UpdatePsdExtensionsRegisterStatus();
    }

    private void UpdatePsdExtensionsRegisterStatus()
    {
        using RegistryKey? clsid = Registry.ClassesRoot.OpenSubKey("CLSID");
        if (clsid != null)
        {
            using RegistryKey? psdExtensionsKey = clsid.OpenSubKey(CommonValues.PsdExtensionsGuid);
            if (psdExtensionsKey != null)
            {
                using RegistryKey? inProcServer32 = psdExtensionsKey.OpenSubKey("InprocServer32");
                if (inProcServer32 != null)
                {
                    string? path = inProcServer32.GetValue(null)?.ToString();

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        FileInfo inRegistry = new(path);
                        FileInfo current = new(CommonValues.PsdExtensionsDllPath);

                        if (inRegistry.Exists && current.Exists && inRegistry.FullName == current.FullName)
                        {
                            IsPsdExtensionsRegistered = true;
                            IsPsdExtensionsRequireReinstall = false;

                            DeterminePsdExtensionActionButton();
                            DeterminePsdExtensionsStatus();
                            return;
                        }
                        else
                        {
                            IsPsdExtensionsRequireReinstall = true;
                        }
                    }
                }
            }
        }

        IsPsdExtensionsRegistered = false;
        DeterminePsdExtensionActionButton();
        DeterminePsdExtensionsStatus();

        void DeterminePsdExtensionActionButton()
        {
            if (IsPsdExtensionsRequireReinstall)
            {
                PsdExtensionActionButtonText = "重新安装扩展";
                PsdExtensionActionButtonCommand = ReinstallExtensionsCommand;
            }
            else if (IsPsdExtensionsRegistered)
            {
                PsdExtensionActionButtonText = "卸载扩展";
                PsdExtensionActionButtonCommand = UninstallPsdExtensionsCommand;
            }
            else
            {
                PsdExtensionActionButtonText = "安装扩展";
                PsdExtensionActionButtonCommand = InstallPsdExtensionsCommand;
            }
        }

        void DeterminePsdExtensionsStatus()
        {
            if (IsPsdExtensionsRequireReinstall)
            {
                PsdExtensionStatusText = "需要重新安装";
                PsdExtensionStatusTextBrush = new SolidColorBrush(Color.FromRgb(0xb2, 0xb2, 0x00));
            }
            else if (IsPsdExtensionsRegistered)
            {
                PsdExtensionStatusText = "已安装";
                PsdExtensionStatusTextBrush = new SolidColorBrush(Color.FromRgb(0x6c, 0xcb, 0x5f));
            }
            else
            {
                PsdExtensionStatusText = "未安装";
                object fillColorObject = Application.Current.Resources["TextFillColorPrimary"];

                Color fillColor = fillColorObject is Color color ? color : Colors.Black;
                PsdExtensionStatusTextBrush = new SolidColorBrush(fillColor);
            }
        }
    }

    [RelayCommand]
    private async Task InstallPsdExtensions()
    {
        await RegisterPsdExtensions();
        UpdatePsdExtensionsRegisterStatus();
    }

    [RelayCommand]
    private async Task UninstallPsdExtensions()
    {
        await UnregisterPsdExtensions();
        UpdatePsdExtensionsRegisterStatus();
    }

    [RelayCommand]
    private async Task ReinstallExtensions()
    {
        Process reinstallProcess = new();
        ProcessStartInfo info = new()
        {
            Verb = "runas",
            Arguments = "/reinstall-extension",
            UseShellExecute = true,
            WorkingDirectory = Environment.ProcessPath,
            FileName = Path.ChangeExtension(typeof(MainWindow).Assembly.Location, ".exe")
        };

        reinstallProcess.StartInfo = info;

        try
        {
            reinstallProcess.Start();
            await reinstallProcess.WaitForExitAsync();
        }
        catch (Win32Exception win32) when (win32.NativeErrorCode == 1223)
        {
            // 用户取消了操作。
        }
        catch (Exception ex)
        {
            DisplayError(ex);
        }

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
            ? $"{CommonValues.PsdExtensionsDllPath} /u"
            : CommonValues.PsdExtensionsDllPath;

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

}