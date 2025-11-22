using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace PsdExtensions.Wpf;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Any(arg => arg == "/reinstall-extension"))
        {
            ReinstallExtension();
            return;
        }

        App app = new();
        app.InitializeComponent();
        app.Run();
    }

    private static void ReinstallExtension()
    {
        CallRegSvr32(true);
        (bool IsCalledRegSvr32Successfully, int ExitCode) = CallRegSvr32();
        if (IsCalledRegSvr32Successfully)
        {
            switch (ExitCode)
            {
                case 5:
                    DisplayRunAsAdmin();
                    break;
                case 0:
                    break;
                default:
                    DisplayRegSvr32Error(ExitCode);
                    break;
            }
        }
    }

    private static (bool IsCalledRegSvr32Successfully, int ExitCode) CallRegSvr32(bool isUnregister = false)
    {
        string argument = isUnregister
            ? $"{CommonValues.PsdExtensionsDllPath} /u"
            : CommonValues.PsdExtensionsDllPath;

        Process regsvr32 = new();
        ProcessStartInfo info = new()
        {
            Arguments = $"{argument} /s",
            FileName = "regsvr32.exe"
        };

        regsvr32.StartInfo = info;

        try
        {
            regsvr32.Start();
            regsvr32.WaitForExit();
            int exitCode = regsvr32.ExitCode;
            return (true, exitCode);
        }
        catch (Win32Exception win32) when (win32.NativeErrorCode == 1223) // 用户取消了操作。
        {
            // ;-)
        }
        catch (Exception ex)
        {
            DisplayError(ex);
        }

        return (false, 0);
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
