using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PsdExtensions.CSharp;

public unsafe class PsdPropertyHelper
{
    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);
    internal const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)0x80040111);
    internal const int ERROR_ALREADY_INITIALIZED = unchecked((int)0x800704df);
    internal const int ERROR_NOT_SUPPORTED = unchecked((int)0x80070032);
    internal const int STG_E_ACCESSDENIED = unchecked((int)0x80030005);
    internal const int E_UNEXPECTED = unchecked((int)0x8000FFFF);
    internal const int E_FAIL = unchecked((int)0x80004005);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int SELFREG_E_CLASS = unchecked((int)0x80040201);

    internal const int S_OK = 0;
    internal const int S_FALSE = 1;

    public static (double PsdX, double PsdY, ResolutionUnit PsdUnit, short PsdRealLayerCount) GetPsdProperties(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(stream);
        ExifDirectoryBase? ifd0Dir = (ExifDirectoryBase?)directories.FirstOrDefault(dir => dir is ExifDirectoryBase);

        if (ifd0Dir != null)
        {
            Rational rationalX = ifd0Dir.GetRational(ExifDirectoryBase.TagXResolution);
            Rational rationalY = ifd0Dir.GetRational(ExifDirectoryBase.TagYResolution);

            ResolutionUnit unit = ifd0Dir.GetUInt32(ExifDirectoryBase.TagResolutionUnit) switch
            {
                2 => ResolutionUnit.Inch,
                3 => ResolutionUnit.Centimeter,
                _ => throw new NotImplementedException("尚未实现其他 DPI 单位的支持。")
            };
            double x = rationalX.ToDouble();
            double y = rationalY.ToDouble();
            short layerCount = ReadLayerCount(stream);

            return (x, y, unit, layerCount);
        }
        else
        {
            throw new NotImplementedException("目前不支持 ifd0 信息不存在的情况。");
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "GetPsdProperties")]
    private static int GetPsdProperties(nint str, uint grfModeInt, double* x, double* y, short* unit, short* layerCount)
    {
        if (str == nint.Zero)
        {
            return E_UNEXPECTED;
        }

        try
        {
            string? path = Marshal.PtrToStringAuto(str);

            if (path is null)
            {
                return E_UNEXPECTED;
            }

            (*x, *y, ResolutionUnit unitEnum, *layerCount) = GetPsdProperties(path);
            *unit = (short)unitEnum;

            return S_OK;
        }
        catch (Exception ex)
        {
            return Marshal.GetHRForException(ex);
        }
    }

    private static short ReadLayerCount(Stream stream)
    {
        // 我们不需要“图层和蒙版信息分区的长度”。
        // 对于 PSB 文件，这些长度尺寸为 8 字节，不过我们这里也用不到 PSB 就是了......
        // 更多信息请查阅：https://www.adobe.com/devnet-apps/photoshop/fileformatashtml/
        stream.Position += 4;

        short count = ReadLayerInfoSectionForRealCount(stream);

        return count;
    }

    private static short ReadLayerInfoSectionForRealCount(Stream stream)
    {
        // Length of the layers info section.
        stream.Position += 4;

        short realCount = ReadLayerRecordsForRealLayerCount(stream);
        return realCount;
    }

    private static short ReadLayerRecordsForRealLayerCount(Stream stream)
    {
        short totalLayerCount = Math.Abs(stream.ReadInt16());
        short folderCount = 0;

        for (int i = 0; i < totalLayerCount; i++)
        {
            SkipLayerRecordsBody(stream);

            uint extraDataLength = stream.ReadUInt32();
            if (extraDataLength == 0)
            {
                continue;
            }

            SkipMaskDataSection(stream);
            SkipBlendingRanges(stream);
            SkipLayerName(stream);

            do
            {
                string signature = stream.ReadString(4, Encoding.UTF8);
                if (signature != "8BIM" && signature != "8B64")
                {
                    stream.Position -= 4;
                    break;
                }

                string key = stream.ReadString(4, Encoding.UTF8);
                // PSB 格式中，某些信息的长度需要使用 ulong 来表示。
                uint dataLength = stream.ReadUInt32();

                if (key != "lsct")
                {
                    stream.Position += dataLength;
                }
                else
                {
                    // 4 possible values:
                    // 0 = any other type of layer,
                    // 1 = open "folder",
                    // 2 = closed "folder",
                    // 3 = bounding section divider, hidden in the UI
                    uint sectionDividerType = stream.ReadUInt32();

                    if (sectionDividerType != 0)
                    {
                        folderCount++;
                    }

                    if (dataLength >= 12)
                    {
                        // Signature.
                        stream.Position += 4;
                        // Key.
                        stream.Position += 4;
                    }

                    if (dataLength >= 16)
                    {
                        // Sub type.
                        stream.Position += 4;
                    }
                }

                const byte padding = 1;
                uint paddingLength = dataLength % padding;
                if (paddingLength > 0)
                {
                    stream.Position += paddingLength;
                }
            } while (true);
        }

        return (short)(totalLayerCount - folderCount);
    }

    private static void SkipLayerName(Stream stream)
    {
        const byte alignment = 4;

        byte stringLength = stream.ReadByteExactly();
        if (stringLength == 0)
        {
            stream.Position += alignment - 1;
            return;
        }

        stream.Position += stringLength;

        int byteSkipped = stringLength + 1;
        int remainingPadding = byteSkipped % alignment;
        if (remainingPadding != 0)
        {
            stream.Position += alignment - remainingPadding;
        }
    }

    private static void SkipBlendingRanges(Stream stream)
    {
        uint blendingRangesLength = stream.ReadUInt32();
        if (blendingRangesLength != 0)
        {
            stream.Position += blendingRangesLength;
        }
    }

    private static void SkipMaskDataSection(Stream stream)
    {
        uint maskDataLength = stream.ReadUInt32();
        if (maskDataLength != 0)
        {
            stream.Position += maskDataLength;
        }
    }

    private static void SkipLayerRecordsBody(Stream stream)
    {
        // Rectangle containing the contents of the layer.
        stream.Position += 16;

        ushort channelCount = stream.ReadUInt16();
        // Channel information. Six bytes per channel.
        stream.Position += 6 * channelCount;

        // Blend mode signature.
        stream.Position += 4;
        // Blend mode key.
        stream.Position += 4;
        // Opacity.
        stream.Position += 1;
        // Clipping.
        stream.Position += 1;
        // Flags.
        stream.Position += 1;
        // Filler.
        stream.Position += 1;
    }
}

file static class PsdStreamExtensions
{
    [DebuggerStepThrough]
    public static string ReadString(this Stream stream, int byteCount, Encoding encoding)
    {
        Span<byte> buffer = byteCount > 256
            ? new byte[byteCount]
            : stackalloc byte[byteCount];

        stream.ReadExactly(buffer);

        return encoding.GetString(buffer);
    }

    [DebuggerStepThrough]
    public static short ReadInt16(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[2];
        stream.ReadExactly(bytes);

        return BinaryPrimitives.ReadInt16BigEndian(bytes);
    }

    [DebuggerStepThrough]
    public static ushort ReadUInt16(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[2];
        stream.ReadExactly(bytes);

        return BinaryPrimitives.ReadUInt16BigEndian(bytes);
    }

    [DebuggerStepThrough]
    public static uint ReadUInt32(this Stream stream)
    {
        Span<byte> bytes = stackalloc byte[4];
        stream.ReadExactly(bytes);

        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    [DebuggerStepThrough]
    public static byte ReadByteExactly(this Stream stream)
    {
        int val = stream.ReadByte();
        if (val == -1)
        {
            throw new EndOfStreamException();
        }

        return (byte)val;
    }
}
