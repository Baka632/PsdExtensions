using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using PsdExtensions.Interop;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.System.Com.StructuredStorage;
using Windows.Win32.System.Variant;
using Windows.Win32.UI.Shell.PropertiesSystem;

namespace PsdExtensions;

[GeneratedComClass]
[Guid("0987EFE0-CDE9-478E-87E3-511E76FDEBFF")]
internal unsafe partial class WindowsExplorerPropertyProvider : IInitializeWithStream, IPropertyStore, IPropertyStoreCapabilities
{
    private bool isInitialized;

    private static readonly IReadOnlyList<ValueTuple<PropertyType, PROPERTYKEY>> SupportedProps = [
            (PropertyType.HorizontalResolution, new PROPERTYKEY() { fmtid = new("6444048F-4C8B-11D1-8B70-080036B11A03"), pid = 5 }),
            (PropertyType.VerticalResolution, new PROPERTYKEY() { fmtid = new("6444048F-4C8B-11D1-8B70-080036B11A03"), pid = 6 }),
            (PropertyType.ResolutionUnit, new PROPERTYKEY() { fmtid = new("19B51FA6-1F92-4A5C-AB48-7DF0ABD67444"), pid = 100 }),
        ];

    public double XResolution { get; private set; }
    public double YResolution { get; private set; }
    public ResolutionUnit Unit { get; private set; }

    public WindowsExplorerPropertyProvider()
    {
        EntryPoint.DllAddRef();
    }

    ~WindowsExplorerPropertyProvider()
    {
        EntryPoint.DllRelease();
    }

    public void GetAt(uint index, PROPERTYKEY* key)
    {
        if (index >= SupportedProps.Count)
        {
            Marshal.ThrowExceptionForHR(E_INVALIDARG);
        }

        (PropertyType, PROPERTYKEY) value = SupportedProps[(int)index];
        *key = value.Item2;
    }

    public void GetCount(out uint count)
    {
        count = (uint)SupportedProps.Count;
    }

    public void GetValue(PROPERTYKEY* keyPtr, out PROPVARIANT pv)
    {
        PROPERTYKEY key = *keyPtr;

        PROPVARIANT emptyPropVariant = new();
        emptyPropVariant.Anonymous.Anonymous.vt = VARENUM.VT_EMPTY;

        if (!SupportedProps.Any(tuple => PropVariantEquals(tuple.Item2, key)))
        {
            pv = emptyPropVariant;
        }
        else
        {
            (PropertyType type, PROPERTYKEY _) = SupportedProps.First(tuple => PropVariantEquals(tuple.Item2, key));

            pv = type switch
            {
                PropertyType.HorizontalResolution => GetHorizontalResolutionPropVariant(),
                PropertyType.VerticalResolution => GetVerticalResolutionPropVariant(),
                PropertyType.ResolutionUnit => GetResolutionUnitVariant(),
                _ => emptyPropVariant
            };
        }

        PROPVARIANT GetHorizontalResolutionPropVariant()
        {
            PROPVARIANT variant = new();
            variant.Anonymous.Anonymous.vt = VARENUM.VT_R8;
            variant.Anonymous.Anonymous.Anonymous.dblVal = XResolution;

            return variant;
        }

        PROPVARIANT GetVerticalResolutionPropVariant()
        {
            PROPVARIANT variant = new();
            variant.Anonymous.Anonymous.vt = VARENUM.VT_R8;
            variant.Anonymous.Anonymous.Anonymous.dblVal = YResolution;

            return variant;
        }

        PROPVARIANT GetResolutionUnitVariant()
        {
            PROPVARIANT variant = new();
            variant.Anonymous.Anonymous.vt = VARENUM.VT_I2;
            variant.Anonymous.Anonymous.Anonymous.iVal = Unit switch
            {
                ResolutionUnit.None => 1,
                ResolutionUnit.Inch => 2,
                ResolutionUnit.Centimeter => 3,
                _ => 1
            };

            return variant;
        }

        static bool PropVariantEquals(PROPERTYKEY left, PROPERTYKEY right)
        {
            return left.pid == right.pid && left.fmtid == right.fmtid;
        }
    }

    public void Initialize(IStream stream, uint grfModeInt)
    {
        STGM grfMode = (STGM)grfModeInt;

        if (isInitialized)
        {
            Marshal.ThrowExceptionForHR(ERROR_ALREADY_INITIALIZED);
        }

        if (stream is null)
        {
            ThrowUnexpected();
        }

        ComStream comStream = new(stream);
        comStream.Seek(0, SeekOrigin.Begin);

        if (!comStream.CanWrite && (grfMode & STGM.STGM_READWRITE) == STGM.STGM_READWRITE)
        {
            Marshal.ThrowExceptionForHR(STG_E_ACCESSDENIED);
        }

        try
        {
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(comStream);
            ExifDirectoryBase? ifd0Dir = (ExifDirectoryBase?)directories.FirstOrDefault(dir => dir is ExifDirectoryBase);

            if (ifd0Dir != null)
            {
                ResolutionUnit unit = ifd0Dir.GetUInt32(ExifDirectoryBase.TagResolutionUnit) switch
                {
                    2 => ResolutionUnit.Inch,
                    3 => ResolutionUnit.Centimeter,
                    _ => throw new NotImplementedException("尚未实现其他 DPI 单位的支持。")
                };
                Rational rationalX = ifd0Dir.GetRational(ExifDirectoryBase.TagXResolution);
                Rational rationalY = ifd0Dir.GetRational(ExifDirectoryBase.TagYResolution);

                XResolution = rationalX.ToDouble();
                YResolution = rationalY.ToDouble();
                Unit = unit;
            }
            else
            {
                ThrowFail();
            }
        }
        catch (Exception)
        {
            ThrowFail();
        }

        isInitialized = true;
    }

    public void SetValue(PROPERTYKEY* key, in PROPVARIANT propVar)
    {
        // 我们不会更改 PSD 中的值。
        Marshal.ThrowExceptionForHR(STG_E_ACCESSDENIED);
    }

    public HRESULT IsPropertyWritable(PROPERTYKEY* key)
    {
        // 我们不会更改 PSD 中的值。
        return new(S_FALSE);
    }

    public void Commit()
    {
        // 我们不会更改 PSD 中的值。
        Marshal.ThrowExceptionForHR(STG_E_ACCESSDENIED);
    }

#pragma warning disable CS8763
    [DoesNotReturn]
    private static void ThrowUnexpected()
    {
        Marshal.ThrowExceptionForHR(E_UNEXPECTED);
    }

    [DoesNotReturn]
    private static void ThrowFail()
    {
        Marshal.ThrowExceptionForHR(E_FAIL);
    }
#pragma warning restore CS8763
}
