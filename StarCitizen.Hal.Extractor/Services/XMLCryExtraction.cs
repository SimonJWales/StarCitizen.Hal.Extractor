
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using unforge;

namespace Hal.Extractor.Services
{
    public class XMLCryExtraction(AppState appState)
    {
        AppState AppState { get; set; } = appState;

        /// <summary>
        /// Begin converting XML files
        /// </summary>
        /// <returns></returns>
        public async Task ConvertXmlFilesAsync(List<string> archiveList)
        {
            int successfullConversions = 0;

            int failedConversions = 0;

            await Parallel.ForEachAsync(
                archiveList,
                Parameters.ParallelOptions,
                async (file, token) =>
                {

                    if (Parameters.CancelTokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    CultureInfo previousCulture = Thread.CurrentThread.CurrentCulture;

                    CultureInfo previousUICulture = Thread.CurrentThread.CurrentUICulture;

                    try
                    {
                        token.ThrowIfCancellationRequested();

                        Thread.CurrentThread.CurrentCulture = Parameters.CultureInfo!;

                        Thread.CurrentThread.CurrentUICulture = Parameters.CultureInfo!;

                        Parameters.NumberFormater.NumberDecimalSeparator = Parameters.CultureSeparate;

                        bool savedOK = await SmeltAsync(file);

                        if (savedOK)
                        {
                            Interlocked.Increment(ref successfullConversions);

                            AppState!.UpdateConvertedCount(successfullConversions);
                        }

                        if (!savedOK)
                        {
                            Interlocked.Increment(ref failedConversions);
                        }

                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine($"CryXML extraction was canceled while parallel");

                        return;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    finally
                    {
                        Thread.CurrentThread.CurrentCulture = previousCulture;

                        Thread.CurrentThread.CurrentUICulture = previousUICulture;
                    }
                });
        }

        /// <summary>
        /// Convert files that are CryXML types and DCB files to a readable file format
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        static async Task<bool> SmeltAsync(
            string outputFile)
        {
            try
            {
                if (Parameters.CancelTokenSource.IsCancellationRequested)
                {
                    return false;
                }

                string extension = Path.GetExtension(outputFile).ToLowerInvariant();

                if (extension is ".dcb")
                {
                    //return await ConvertDcbToXmlAsync(outputFile);

                    return ConvertDcbToXml(outputFile);
                }

                if (extension is ".socpak")
                {
                    return UnSocpakAsync(outputFile);
                }

                if (extension is ".xml" ||
                    extension is ".entxml" ||
                    extension is ".bspace")
                {
                    //return await ProcessXmlFileAsync(outputFile);

                    return ProcessXmlFile(outputFile);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"Smelt extraction was canceled while processing {outputFile}");

                return false;  // Indicate that the operation was canceled
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Smelt extraction file error {outputFile}: {ex}");

                return false;
            }

            return true;
        }

        static bool UnSocpakAsync(string socpakFile)
        {
            string directory = Path.GetDirectoryName(socpakFile)!;

            string fileNameWithoutExt = $"{Path.GetFileNameWithoutExtension(socpakFile)}-socpak";

            // combine them to get the full path without extension
            string newSocpakOutputPath = Path.Combine(
                directory,
                fileNameWithoutExt);

            ZipFile? zipFile = null;

            try
            {
                FileStream fs = File.OpenRead(socpakFile);

                zipFile = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }
                    
                    string entryFileName = zipEntry.Name;

                    // adjust directory separators to match the operating system
                    entryFileName = entryFileName.Replace(
                        '/', 
                        Path.DirectorySeparatorChar);

                    byte[] buffer = new byte[4096];

                    Stream zipStream = zipFile.GetInputStream(zipEntry);

                    // create full directory path
                    string fullZipToPath = Path.Combine(
                        newSocpakOutputPath, 
                        entryFileName);

                    string directoryName = Path.GetDirectoryName(fullZipToPath)!;

                    if (directoryName.Length > 0)

                        Directory.CreateDirectory(directoryName);

                    using FileStream streamWriter = File.Create(fullZipToPath);

                    ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(
                        zipStream, 
                        streamWriter, 
                        buffer);
                }
            }
            finally
            {
                if (zipFile != null)
                {
                    // close also closes the underlying stream
                    zipFile.IsStreamOwner = true;

                    // release resources
                    zipFile.Close();
                }
            }

            return true;
        }

        /// <summary>
        /// Convert DCB files to XML and save to disk
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        static bool ConvertDcbToXml(string outputFile)
        {
            if (Parameters.CancelTokenSource.IsCancellationRequested)
            {
                return false;
            }

            using var binaryReader = new BinaryReader(File.OpenRead(outputFile));

            bool legacy = new FileInfo(outputFile).Length < 0x0e2e00;

            DataForge dataForge = new(
                binaryReader,
                legacy);

            string xmlPath = Path.ChangeExtension(
                outputFile,
                "xml");

            dataForge.Save(xmlPath);

            return true;
        }

        /// <summary>
        /// Convert DCB files async to XML and save to disk
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        static Task<bool> ConvertDcbToXmlAsync(string outputFile)
        {
            var task = Task.Run(() =>
            {
                if (Parameters.CancelTokenSource.IsCancellationRequested)
                {
                    return false;
                }

                using var binaryReader = new BinaryReader(File.OpenRead(outputFile));

                bool legacy = new FileInfo(outputFile).Length < 0x0e2e00;

                DataForge dataForge = new(
                    binaryReader,
                    legacy);

                string xmlPath = Path.ChangeExtension(
                    outputFile,
                    "xml");

                dataForge.Save(xmlPath);

                return true;
            });

            task.Wait();

            return task;
        }

        /// <summary>
        /// Process XML files and save to disk
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        static bool ProcessXmlFile(string outputFile)
        {
            if (Parameters.CancelTokenSource.IsCancellationRequested)
            {
                return false;
            }

            XmlDocument xml = CryXmlSerializer.ReadFile(outputFile);


            string xmlPath = outputFile;

            if (!Path.GetExtension(xmlPath).Equals(
                ".xml",
                StringComparison.CurrentCultureIgnoreCase))
            {
                xmlPath += ".xml";
            }

            //Path.ChangeExtension(
            //    outputFile,
            //    "xml");

            xml?.Save(xmlPath);

            return xml != null;
        }

        /// <summary>
        /// Process XML files async and save to disk
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        static Task<bool> ProcessXmlFileAsync(string outputFile)
        {
            var task = Task.Run(() =>
            {
                if (Parameters.CancelTokenSource.IsCancellationRequested)
                {
                    return false;
                }

                XmlDocument xml = CryXmlSerializer.ReadFile(outputFile);


                string xmlPath = outputFile;

                if (!Path.GetExtension(xmlPath).Equals(
                    ".xml", 
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    xmlPath += ".xml";
                }

                //Path.ChangeExtension(
                //    outputFile,
                //    "xml");

                xml?.Save(xmlPath);

                return xml != null;
            });

            task.Wait();

            return task;
        }
    }
}
