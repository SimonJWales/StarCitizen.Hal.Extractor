
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

        public static readonly string ExtractFromPreference = "P4KPath";

        public static readonly string ExtractToPreference = "ExtractedPath";

        public static readonly string[] AssetSeparator = ["\r\n", "\r", "\n"];

        public static readonly Dictionary<string, List<string>> Defaults = new()
        {
            {
                "Default data", 
                new List<string>
                {
                    ".dcb",
                    ".ini",
                    ".json",
                    ".txt",
                    ".xml"
                }
            },
            {
                "Images",
                new List<string>
                {
                    ".dds",
                    ".img",
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".svg",
                    ".tif",
                    ".tiff"
                }
            },
            {
                "Media (video/audio)",
                new List<string>
                {
                    ".bk2",
                    ".bnk",
                    ".ogg",
                    ".raw",
                    ".svg",
                    ".ttf",
                    ".tiff",
                    ".wem"
                }
            },
            {
                "ALL (warning - many minutes)",
                new List<string>()
            }
        };
    }
}
