using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.System.Com;

namespace PsdExtensions.Interop;

[GeneratedComInterface]
[Guid("0000000c-0000-0000-C000-000000000046")]
internal unsafe partial interface IStream : ISequentialStream
{
    void Seek(long dlibMove, SeekOrigin dwOrigin, out ulong plibNewPosition);

    void SetSize(ulong libNewSize);

    void Commit(STGC grfCommitFlags);

    void Stat(STATSTG* pstatstg, STATFLAG grfStatFlag);

    void Clone(out IStream ppstm);
}
