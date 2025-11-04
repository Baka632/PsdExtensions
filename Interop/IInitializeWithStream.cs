using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.System.Com;

namespace PsdExtensions;

[GeneratedComInterface]
[Guid("b824b49d-22ac-4161-ac8a-9916e8fa3f7f")]
internal partial interface IInitializeWithStream
{
    void Initialize(Interop.IStream stream, STGM grfMode);
}
