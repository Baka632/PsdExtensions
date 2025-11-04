using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.System.Com;

namespace PsdExtensions.Interop;

[GeneratedComClass]
internal partial class ComClassFactory : IClassFactory
{
    public unsafe void CreateInstance(nint pUnkOuter, in Guid riid, out nint ppvObject)
    {
        ppvObject = nint.Zero;

        if (pUnkOuter != nint.Zero)
        {
            Marshal.ThrowExceptionForHR(CLASS_E_NOAGGREGATION);
        }

        if (riid == typeof(IUnknown).GUID
            || riid == typeof(IInitializeWithStream).GUID
            || riid == typeof(IPropertyStore).GUID
            || riid == typeof(IPropertyStoreCapabilities).GUID)
        {
            WindowsExplorerPropertyProvider provider = new();

            nint ptr = DefaultComWrappers.GetOrCreateComInterfaceForObject(provider, CreateComInterfaceFlags.None);
            int result = Marshal.QueryInterface(ptr, in riid, out ppvObject);
            Marshal.Release(ptr);

            Marshal.ThrowExceptionForHR(result);
        }
        else
        {
            Marshal.ThrowExceptionForHR(E_NOINTERFACE);
        }
    }

    public void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock)
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
