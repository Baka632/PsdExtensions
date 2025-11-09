using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Runtime.InteropServices;
using PsdExtensions.CSharp.Interop;
using Windows.Win32.System.Com;

namespace PsdExtensions.CSharp;

internal unsafe class PsdPropertyHelper
{
    [UnmanagedCallersOnly(EntryPoint = "GetPsdProperties")]
    private static int GetPsdProperties(nint ptr, uint grfModeInt, double* x, double* y, short* unit)
    {
        STGM grfMode = (STGM)grfModeInt;

        if (ptr == nint.Zero)
        {
            return E_UNEXPECTED;
        }

        IStream stream = (IStream)DefaultComWrappers.GetOrCreateObjectForComInstance(ptr, CreateObjectFlags.None);
        ComStream comStream = new(stream);
        comStream.Seek(0, SeekOrigin.Begin);

        if (!comStream.CanWrite && (grfMode & STGM.STGM_READWRITE) == STGM.STGM_READWRITE)
        {
            return STG_E_ACCESSDENIED;
        }

        try
        {
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(comStream);
            ExifDirectoryBase? ifd0Dir = (ExifDirectoryBase?)directories.FirstOrDefault(dir => dir is ExifDirectoryBase);

            if (ifd0Dir != null)
            {
                ResolutionUnit unitEnum = ifd0Dir.GetUInt32(ExifDirectoryBase.TagResolutionUnit) switch
                {
                    2 => ResolutionUnit.Inch,
                    3 => ResolutionUnit.Centimeter,
                    _ => throw new NotImplementedException("尚未实现其他 DPI 单位的支持。")
                };
                Rational rationalX = ifd0Dir.GetRational(ExifDirectoryBase.TagXResolution);
                Rational rationalY = ifd0Dir.GetRational(ExifDirectoryBase.TagYResolution);

                *x = rationalX.ToDouble();
                *y = rationalY.ToDouble();
                *unit = (short)unitEnum;

                return S_OK;
            }
            else
            {
                return E_FAIL;
            }
        }
        catch (Exception)
        {
            return E_FAIL;
        }
    }
}
