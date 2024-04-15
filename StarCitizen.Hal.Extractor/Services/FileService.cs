
using Hal.Extractor.Entities;
using Microsoft.Win32;
using System.Text.Json;

namespace Hal.Extractor.Services;

public class FileService
{
    static readonly string _registryPath = @"Software\HAL Extractor";

    public static void SaveToRegistry(
        string valuePath,
        string valueData)
    {
        if (OperatingSystem.IsWindows())
        {
            using var key = Registry.CurrentUser.CreateSubKey(_registryPath);

            key.SetValue(
                valuePath,
                valueData);
        }
    }

    public static string? ReadFromRegistry(string valuePath)
    {
        if (OperatingSystem.IsWindows())
        {
            using var key = Registry.CurrentUser.OpenSubKey(_registryPath);

            if (key is null)
            {
                return string.Empty;
            }

            var value = key.GetValue(valuePath);

            return value?.ToString();
        }

        return string.Empty;
    }


    /// <summary>
    /// Get the branch and build data from the build manifest file
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public static async Task<VersionData> GetGameVersionData(string p4kFile)
    {
        var versionJson = await GetFile(
            Path.Combine(
                Path.GetDirectoryName(p4kFile)!,
                Parameters.VersionFile));

        if (string.IsNullOrEmpty(versionJson))
        {
            return new();
        }

        var versionData = JsonSerializer
            .Deserialize<VersionData>(versionJson);

        if (versionData?.Data is null)
        {
            return new();
        }

        return versionData;
    }

    public static async Task<string?> GetFile(string fileName)
    {
        return await File.ReadAllTextAsync(fileName);
    }

    public static async Task WriteTFile<T>(
        string fileName,
        T data)
    {
        var json = JsonSerializer.Serialize(data);

        await File.WriteAllTextAsync(
            fileName,
            json);
    }
}
