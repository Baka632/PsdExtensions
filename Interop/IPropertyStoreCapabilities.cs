using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;

namespace PsdExtensions.Interop;

[GeneratedComInterface]
[Guid("c8e2d566-186e-4d49-bf41-6909ead56acc")]
internal partial interface IPropertyStoreCapabilities
{
    [PreserveSig]
    int IsPropertyWritable(in PROPERTYKEY key);
}
