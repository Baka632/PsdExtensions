using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace PsdExtensions.Interop;

[GeneratedComInterface]
[Guid("00000001-0000-0000-C000-000000000046")]
public partial interface IClassFactory
{
    void CreateInstance(nint pUnkOuter, in Guid riid, out nint ppvObject);
    void LockServer([MarshalAs(UnmanagedType.Bool)] bool fLock);
}
