
using Hal.Extractor.Entities;
using System.Globalization;

namespace Hal.Extractor.Services
{
    public static class Parameters
    {
        public static CancellationTokenSource CancelTokenSource { get; set; } = new();

        public static readonly CancellationToken CancelToken = CancelTokenSource.Token;

        public static readonly ParallelOptions ParallelOptions = new()
        {
            MaxDegreeOfParallelism = 3
        };

        public static CultureInfo? CultureInfo { get; set; } = new("en-GB");
         
        public static readonly string CultureSeparate = ".";
         
        public static readonly NumberFormatInfo NumberFormater = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();

        public static readonly string Title = "HAL Extractor";

        public static readonly string[] WinDevicePlatform = [".p4k"];

        public static readonly string VersionFile = "build_manifest.id";

        public static readonly string PickerP4K = "Select the Star Citizen Data.p4k file";

        public static readonly string ExtensionsFile = "Extensions.txt";

        public static readonly string ExtractFromRegistryName = "P4KPath";

        public static readonly string ExtractToRegistryName = @"ExtractedPath";

        public static readonly string ExtractTypesXML = "";

        public static readonly string ExtractTypesImages = "";

        public static readonly string ExtractTypesAll = "ALL (Warning - many minutes)";
        
        public static readonly string[] AssetSeparator = ["\r\n", "\r", "\n"];

        public static readonly List<string> DefaultTypes =
        [
            "Default XML",
            "Default Images",
            "ALL (Warning - many minutes)"
        ];

        public static readonly List<string> DefaultXMLExtensions =
        [
            //".obj",
            ".ini",
            ".dcb",
            ".xml",
            ".txt",
            ".json"
        ];

        public static readonly List<string> DefaultImageExtensions =
        [
            ".dds",
            ".jpg",
            ".jpeg",
            ".png",
            ".svg",
            ".tif",
            ".tiff",
            ".img"
        ];
    }
}
