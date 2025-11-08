using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell.PropertiesSystem;

namespace PsdExtensions.Interop;

[GeneratedComClass]
internal partial class ComClassFactory : IClassFactory
{
    public unsafe void CreateInstance([MarshalAs(UnmanagedType.Interface)] object pUnkOuter, Guid* riid, [MarshalAs(UnmanagedType.Interface)] out object ppvObject)
    {
        ppvObject = null!;

        if (pUnkOuter != null)
        {
            Marshal.ThrowExceptionForHR(CLASS_E_NOAGGREGATION);
        }

        Guid guid = *riid;
        if (guid == typeof(IUnknown).GUID
            || guid == typeof(IInitializeWithStream).GUID
            || guid == typeof(IPropertyStore).GUID
            || guid == typeof(IPropertyStoreCapabilities).GUID)
        {
            WindowsExplorerPropertyProvider provider = new();
            ppvObject = provider;
        }
        else
        {
            Marshal.ThrowExceptionForHR(E_NOINTERFACE);
        }
    }

    public void LockServer(BOOL fLock)
    {
        if (fLock)
        {
            EntryPoint.DllAddRef();
        }
        else
        {
            EntryPoint.DllRelease();
        }
    }
}
