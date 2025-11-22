using System.IO;

namespace PsdExtensions.Wpf;

internal sealed class CommonValues
{
    public const string PsdExtensionsGuid = "{5f4d0838-ea2a-40af-828f-c24bf8e0d90e}";
    public const string PsdExtensionsDll = "PsdExtensions.Plugin.dll";
    public static readonly string PsdExtensionsDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PsdExtensionsDll);
}
