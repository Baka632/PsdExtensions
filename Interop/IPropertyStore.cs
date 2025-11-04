using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com.StructuredStorage;

namespace PsdExtensions.Interop;

[GeneratedComInterface]
[Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
internal partial interface IPropertyStore
{
    void Commit();
    void GetAt(uint index, out PROPERTYKEY key);
    void GetCount(out uint count);
    void GetValue(in PROPERTYKEY key, out PROPVARIANT pv);
    void SetValue(in PROPERTYKEY key, in PROPVARIANT propVar);
}
