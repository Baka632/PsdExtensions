using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace PsdExtensions.Interop;

[GeneratedComInterface]
[Guid("0c733a30-2a1c-11ce-ade5-00aa0044773d")]
internal partial interface ISequentialStream
{
    void Read(nint pv, uint cb, out uint pcbRead);
    void Write(nint pv, uint cb, out uint pcbWritten);
}