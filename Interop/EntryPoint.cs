using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.WindowsAndMessaging;

namespace PsdExtensions.Interop;

internal static class EntryPoint
{
    private static nint ModuleHandle = nint.Zero;
    private static long moduleRefCount = 0;

    private const uint DLL_PROCESS_DETACH = 0,
                           DLL_PROCESS_ATTACH = 1,
                           DLL_THREAD_ATTACH = 2,
                           DLL_THREAD_DETACH = 3;

    [UnmanagedCallersOnly(EntryPoint = "DllMain", CallConvs = new[] { typeof(CallConvStdcall) })]
    public static bool DllMain(nint hModule, uint ul_reason_for_call, nint lpReserved)
    {
        switch (ul_reason_for_call)
        {
            case DLL_PROCESS_ATTACH:
                ModuleHandle = hModule;
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                break;
            default:
                break;
        }
        return true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogError(e.ExceptionObject);
    }

    [UnmanagedCallersOnly(EntryPoint = "DllCanUnloadNow", CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int DllCanUnloadNow()
    {
        return moduleRefCount == 0 ? S_OK : S_FALSE;
    }

    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject", CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int DllGetClassObject(Guid* clsid, Guid* riid, void** ppv)
    {
        // Get object for Class factory
        *ppv = null;

        if (*clsid != typeof(WindowsExplorerPropertyProvider).GUID)
        {
            return CLASS_E_CLASSNOTAVAILABLE;
        }

		try
		{
            Guid guid = *riid;
            if (guid == typeof(IClassFactory).GUID || guid == typeof(IUnknown).GUID)
            {
                ComClassFactory factory = new();

                nint ptr = DefaultComWrappers.GetOrCreateComInterfaceForObject(factory, CreateComInterfaceFlags.None);
                int result = Marshal.QueryInterface(ptr, in guid, out nint ppvPtr);
                Marshal.Release(ptr);
                *ppv = (void*)ppvPtr;

                return result;
            }
            else
            {
                return E_NOINTERFACE;
            }
        }
		catch (Exception e)
		{
            LogError(e);
            return Marshal.GetHRForException(e);
		}
    }

    [UnmanagedCallersOnly(EntryPoint = "DllRegisterServer", CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int DllRegisterServer()
    {
        try
        {
            string guid = typeof(WindowsExplorerPropertyProvider).GUID.ToString("B");

            using RegistryKey? clsidKey = Registry.LocalMachine.OpenSubKey("Software\\Classes\\CLSID\\", true);
            using RegistryKey? classKey = clsidKey?.CreateSubKey(guid);

            if (classKey is not null)
            {
                classKey.SetValue(null, "Baka632's PSD Property Provider", RegistryValueKind.String);
                using RegistryKey? inProcServer32 = classKey.CreateSubKey("InProcServer32");
                if (inProcServer32 is not null)
                {
                    inProcServer32.SetValue(null, GetModulePath(), RegistryValueKind.String);
                    inProcServer32.SetValue("ThreadingModel", "Both", RegistryValueKind.String);

                    using RegistryKey? propertySystem = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\PropertySystem\\PropertyHandlers\\", true);
                    using RegistryKey? psd = propertySystem?.CreateSubKey(".psd");
                    if (psd is not null)
                    {
                        psd.SetValue(null, guid, RegistryValueKind.String);
                        return S_OK;
                    }
                }
            }
        }
        catch (Exception e)
        {
            LogError(e);
        }

        return SELFREG_E_CLASS;
    }

    [UnmanagedCallersOnly(EntryPoint = "DllUnregisterServer", CallConvs = new[] { typeof(CallConvStdcall) })]
    private static unsafe int DllUnregisterServer()
    {
        try
        {
            string guid = typeof(WindowsExplorerPropertyProvider).GUID.ToString("B");

            using RegistryKey? clsidKey = Registry.LocalMachine.OpenSubKey("Software\\Classes\\CLSID\\", true);
            using RegistryKey? appIDKey = Registry.LocalMachine.OpenSubKey("Software\\Classes\\AppID\\", true);
            using RegistryKey? propertySystem = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\PropertySystem\\PropertyHandlers\\", true);

            appIDKey?.DeleteSubKeyTree(guid, false);
            clsidKey?.DeleteSubKeyTree(guid, false);

            RegistryKey? psd = propertySystem?.OpenSubKey(".psd", true);
            if (psd is not null && guid.Equals(psd.GetValue(null)))
            {
                psd.Dispose();
                propertySystem?.DeleteSubKeyTree(".psd");
            }

            return S_OK;
        }
        catch (Exception e)
        {
            LogError(e);
        }

        return SELFREG_E_CLASS;
    }

    internal static void DllAddRef()
    {
        Interlocked.Increment(ref moduleRefCount);
    }

    internal static void DllRelease()
    {
        Interlocked.Decrement(ref moduleRefCount);
    }

    private static string GetModulePath()
    {
        char[] path = new char[32769];
        using ModuleSafeHandle handle = new(ModuleHandle);
        int length = (int)PInvoke.GetModuleFileName(handle, path);
        return new string(path.AsSpan()[..length]);
    }

    private static void LogError(object obj)
    {
        PInvoke.MessageBox(HWND.Null, obj.ToString(), "Error!", MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONERROR);
    }
}

file class ModuleSafeHandle : SafeHandle
{
    private bool isInvalid;

    public ModuleSafeHandle(nint handle) : base(0, true)
    {
        SetHandle(handle);
    }

    public override bool IsInvalid => isInvalid;

    protected override bool ReleaseHandle()
    {
        isInvalid = true;
        return true;
    }
}
