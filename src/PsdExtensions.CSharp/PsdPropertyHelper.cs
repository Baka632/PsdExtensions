using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Runtime.InteropServices;

namespace PsdExtensions.CSharp;

internal unsafe class PsdPropertyHelper
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

    [UnmanagedCallersOnly(EntryPoint = "GetPsdProperties")]
    private static int GetPsdProperties(nint str, uint grfModeInt, double* x, double* y, short* unit)
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

            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(path);
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
        catch (Exception ex)
        {
            return Marshal.GetHRForException(ex);
        }
    }
}
