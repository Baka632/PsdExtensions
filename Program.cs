//using MetadataExtractor;
//using MetadataExtractor.Formats.Exif;
//using PsdExtensions;

//while (true)
//{
//    Console.WriteLine("Type path");

//    string? path = Console.ReadLine();

//    if (string.IsNullOrWhiteSpace(path))
//    {
//        Console.WriteLine("Exiting...");
//        break;
//    }
//    else if (!File.Exists(path))
//    {
//        Console.WriteLine("Not found");
//        continue;
//    }

//    IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(path);
//    ExifDirectoryBase? ifd0Dir = (ExifDirectoryBase?)directories.FirstOrDefault(dir => dir is ExifDirectoryBase);

//    if (ifd0Dir != null)
//    {
//        ResolutionUnit unit = ifd0Dir.GetUInt32(ExifDirectoryBase.TagResolutionUnit) switch
//        {
//            2 => ResolutionUnit.Inch,
//            3 => ResolutionUnit.Centimeter,
//            _ => throw new NotImplementedException("尚未实现其他 DPI 单位的支持。")
//        };
//        Rational rationalX = ifd0Dir.GetRational(ExifDirectoryBase.TagXResolution);
//        Rational rationalY = ifd0Dir.GetRational(ExifDirectoryBase.TagYResolution);

//        string type = unit switch
//        {
//            ResolutionUnit.Inch => "DPI",
//            ResolutionUnit.Centimeter => "DPCM",
//            _ => throw new NotImplementedException("尚未实现其他 DPI 单位的支持。")
//        };
//        Console.WriteLine($"X {type}: {rationalX.ToDouble()}");
//        Console.WriteLine($"Y {type}: {rationalY.ToDouble()}");
//    }
//    else
//    {
//        Console.WriteLine("None.");
//    }
//}