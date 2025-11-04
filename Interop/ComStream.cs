using Windows.Win32;
using Windows.Win32.System.Com;

namespace PsdExtensions.Interop;

internal unsafe class ComStream(IStream source) : Stream
{
    public override bool CanRead
    {
        get
        {
            STATSTG stat = new();
            source.Stat(&stat, STATFLAG.STATFLAG_NONAME);
            return (stat.grfMode & STGM.STGM_READ) == STGM.STGM_READ;
        }
    }

    public override bool CanSeek => true;

    public override bool CanWrite
    {
        get
        {
            STATSTG stat = new();
            source.Stat(&stat, STATFLAG.STATFLAG_NONAME);
            return (stat.grfMode & STGM.STGM_WRITE) == STGM.STGM_WRITE;
        }
    }

    public override void Flush()
    {
        source.Commit(STGC.STGC_DEFAULT);
    }

    public override long Length
    {
        get
        {
            STATSTG stat = new();
            source.Stat(&stat, STATFLAG.STATFLAG_NONAME);
            return (long)stat.cbSize;
        }
    }

    public override long Position
    {
        get => Seek(0, SeekOrigin.Current);
        set => Seek(value, SeekOrigin.Begin);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        Span<byte> bytes = new(buffer, offset, count);

        fixed (byte* ptr = bytes)
        {
            source.Read((nint)ptr, (uint)count, out uint readCount);
            return (int)readCount;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        source.Seek(offset, origin, out ulong newPosition);
        return (long)newPosition;
    }

    public override void SetLength(long value)
    {
        source.SetSize((ulong)value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Span<byte> span = new(buffer, offset, count);
        fixed (byte* ptr = span)
        {
            source.Write((nint)ptr, (uint)count, out _);
        }
    }
}