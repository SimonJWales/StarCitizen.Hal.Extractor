
using Hal.Extractor.Entities;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Hal.Extractor.Services;

public class ExtractionService(AppState appState)
{
    AppState? AppState { get; set; } = appState;

    /// <summary>
    /// Open the p4k file for reading and extraction of data
    /// </summary>
    /// <param name="path"></param>
    /// <param name="extractionPath"></param>
    /// <param name="fileTypes"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public async Task<List<string>> ExtractAsync(
        string path,
        string extractionPath,
        ObservableCollection<string> fileTypes,     // this is the selected file types for extraction
        List<string>? extensions)                   // this is the list of extensions used for comparison for new file extensions
    {
        string extractTo = await GetFullExtractionPathFromGameVersionData(
            path, 
            extractionPath);

        return await Task.Run(async () =>
        {
            using var p4kFile = File.OpenRead(path);

            var p4kArchive = new ICSharpCode.SharpZipLib.Zip.ZipFile(p4kFile)
            {
                Key = GetBytes()
            };

            byte[] buffer = new byte[4096];

            List<string> archiveList = await ParseArchiveFile(
                p4kArchive,
                buffer,
                extractTo,
                fileTypes,
                extensions);

            return archiveList;
        });
    }

    static async Task<string> GetFullExtractionPathFromGameVersionData(
        string p4kFile, 
        string extractionPath)
    {
        string gameVersion = "UNKNOWN-GAME-VERSION";

        string? directoryName = Path.GetDirectoryName(p4kFile);

        string? gameEnvironment = Path.GetFileName(directoryName);

        if (string.IsNullOrEmpty(gameEnvironment))
        {
            gameEnvironment = "UNKNOWN-ENVIRONMENT";
        }

        VersionData versionData = await FileService.GetGameVersionData(p4kFile);

        if (versionData?.Data?.Version is not null)
        {
            gameVersion = versionData.Data.Version;
        }

        if (versionData?.Data?.Version is null && 
            versionData?.Data?.Branch is not null)
        {
            gameVersion = versionData.Data.Branch;
        }

        return @$"{extractionPath}\{gameEnvironment}_{gameVersion}";
    }

    /// <summary>
    /// Parse the archive file and extract the data
    /// </summary>
    /// <param name="p4kArchive"></param>
    /// <param name="buffer"></param>
    /// <param name="extractionPath"></param>
    /// <param name="fileTypes"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    async Task<List<string>> ParseArchiveFile(
        ICSharpCode.SharpZipLib.Zip.ZipFile p4kArchive,
        byte[] buffer,
        string extractionPath,
        ObservableCollection<string> fileTypes,
        List<string>? extensions)
    {
        List<string> archiveList = [];

        extensions ??= [];

        foreach (ICSharpCode.SharpZipLib.Zip.ZipEntry entry in p4kArchive)
        {
            if (Parameters.CancelTokenSource.IsCancellationRequested)
            {
                return archiveList;
            }

            // more elegant than a scrappy linq query
            bool isValidExtension = (from extensionType in fileTypes
                                         where entry.Name.ToLower().Contains(
                                             extensionType,
                                             StringComparison.CurrentCultureIgnoreCase)
                                         select true)
                                     .FirstOrDefault();

            var ext = Path.GetExtension(entry.Name);

            if (!extensions.Contains(ext.ToLower()))
            {
                extensions.Add(ext.ToLower());
            }

            if (!isValidExtension)
            {
                continue;
            }

            var filePath = $@"{extractionPath}\{entry.Name}";

            bool extractedOK = await ExtractFile(
                filePath,
                p4kArchive,
                entry,
                buffer);

            if (extractedOK)
            {
                archiveList ??= [];

                archiveList!.Add(filePath);

                AppState?.UpdateFileCount(archiveList.Count);
            }

            if (!extractedOK)
            {
                Debug.WriteLine(
                    $"ERROR extracting {entry.Name}");
            }
        }

        return archiveList;
    }

    /// <summary>
    /// Check for cancellation before extracting the file from the archive from an input stream
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="p4kArchive"></param>
    /// <param name="entry"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    static async Task<bool> ExtractFile(
        string filePath,
        ICSharpCode.SharpZipLib.Zip.ZipFile p4kArchive,
        ICSharpCode.SharpZipLib.Zip.ZipEntry entry,
        byte[] buffer)
    {
        try
        {
            if (Parameters.CancelTokenSource.IsCancellationRequested)
            {
                return false;
            }

            bool result = await Task.Run(() => ExtractFileAsync(
                filePath, 
                p4kArchive.GetInputStream(entry), 
                entry, 
                buffer));

            return result;
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Extraction was canceled.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("An error occurred: " + ex.Message);
        }

        return false;
    }

    /// <summary>
    /// Extract the file from the archive from an input stream
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="p4kArchiveStream"></param>
    /// <param name="entry"></param>
    /// <param name="buffer"></param>
    /// <returns></returns>
    static async Task<bool> ExtractFileAsync(
        string filePath,
        Stream p4kArchiveStream,
        ICSharpCode.SharpZipLib.Zip.ZipEntry entry,
        byte[] buffer)
    {
        try
        {
            var target = new FileInfo(filePath);

            if (!target.Directory!.Exists)
            {
                target.Directory.Create();
            }

            using var stream = p4kArchiveStream;

            using var fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            await stream.CopyToAsync(
                fileStream, 
                buffer.Length, 
                Parameters.CancelToken);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ExtractFile exception while decompressing {entry.Name}: {ex}");

            return false;
        }
    }

    /// <summary>
    /// Get the key bytes for the archive
    /// </summary>
    /// <returns></returns>
    static byte[] GetBytes()
    {
        return [
            0x5E,
            0x7A,
            0x20,
            0x02,
            0x30,
            0x2E,
            0xEB,
            0x1A,
            0x3B,
            0xB6,
            0x17,
            0xC3,
            0x0F,
            0xDE,
            0x1E,
            0x47
        ];
    }
}
