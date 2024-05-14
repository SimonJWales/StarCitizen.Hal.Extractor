
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using StarCitizen.Hal.Extractor.Library.Cry;
using StarCitizen.Hal.Extractor.Library.Dolkens.Unforge;
using StarCitizen.Hal.Extractor.Library.Enum;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Hal.Extractor.Services
{
    public class XMLCryExtraction(AppState appState, ILogger log)
    {
        AppState AppState { get; set; } = appState;

        ILogger Log { get; set; } = log;

        readonly int _charsLength = 7;

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

                        bool savedOK = await HalConverter(file);

                        //bool savedOK = await SmeltAsync(file);

                        //bool savedOK = Smelt(file);

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
                        //Log.LogInformation("CryXML extraction was canceled while parallel");

                        return;
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(
                            ex,
                            "Exception Error converting CryXML file: {file}: {ex}",
                            file,
                            ex.Message);

                        return;
                    }
                    finally
                    {
                        Thread.CurrentThread.CurrentCulture = previousCulture;

                        Thread.CurrentThread.CurrentUICulture = previousUICulture;
                    }
                });
        }

        async Task<bool> HalConverter(string file)
        {
            try
            {
                if (Parameters.CancelTokenSource.IsCancellationRequested)
                {
                    return false;
                }

                string extension = Path.GetExtension(file).ToLowerInvariant();

                if (extension is ".dcb")
                {
                    return await ConvertDcbToXmlAsync(file);
                }

                if (extension is ".socpak")
                {
                    return UnSocpakAsync(file);
                }

                if (extension is ".xml" ||
                    extension is ".entxml" ||
                    extension is ".bspace")
                {
                    bool savedOk = HalXml(file);

                    return savedOk;
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Log.LogError(
                    ex,
                    "HalConverter extraction file error {file}: {ex}",
                    file,
                    ex.Message);

                return false;
            }

            return true;
        }

        bool HalXml(string file)
        {
            using FileStream fs = new(
                file,
                FileMode.Open);

            using BinaryReader binaryReader = new(fs);

            int peek = binaryReader.PeekChar();

            bool isXML = peek == '<' || peek == 65279;

            if (isXML)
            {
                // already an xml file (or not anything we want to decrpyt)
                return false;
            }

            if (peek != 'C')
            {
                Log.LogError(
                    "Invalid CryXML peek: {peek} in file: {file}",
                    peek,
                    file);

                return false;
            }

            // read a Fixed-Length string from the stream
            char[] chars = binaryReader.ReadChars(_charsLength);

            string header = ReadHeader(chars);

            bool isCryFile = CheckForCryHeader(
                binaryReader,
                header);

            if (!isCryFile)
            {
                return false;
            }

            long headerLength = binaryReader.BaseStream.Position;

            Endiness endiness = DetermineByteOrder(
                binaryReader,
                headerLength);

            CryMeta meta = GetCryMeta(
                binaryReader,
                endiness);

            CryTable? table = BuildCryTable(
                binaryReader, 
                endiness, 
                meta);

            if (table is null)
            {
                Log.LogError(
                    "Failed to build CryTable for: {file}",
                    file);

                return false;
            }

            table.Map = table.CryData!
                .ToDictionary(k => k.Offset, v => v.Value);

            fs.Close(); // close the underlying stream, so we can save back to the same file

            XmlDocument xml = BuildXmlDocument(table);

            xml?.Save(file);

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
                if (zipFile is not null)
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
        /// Convert DCB files async to XML and save to disk
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        Task<bool> ConvertDcbToXmlAsync(string outputFile)
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

                AppState.UpdateFileCount(AppState.FileCount + 1);

                string xmlPath = Path.ChangeExtension(
                    outputFile,
                    "xml");

                dataForge.Save(
                    xmlPath,
                    AppState);

                AppState.UpdateConvertedCount(AppState.ConvertedCount + 1);

                return true;
            });

            task.Wait();

            return task;
        }

        static CryTable BuildCryTable(BinaryReader binaryReader, Endiness endiness, CryMeta meta)
        {
            return new()
            {
                CryNode = ReadCryNode(
                    binaryReader,
                    meta,
                    endiness),
                CryReference = ReadCryAttributes(
                    binaryReader,
                    meta,
                    endiness),
                Parent = ReadCryParent(
                    binaryReader,
                    meta,
                    endiness),
                CryData = ReadCryData(
                    binaryReader,
                    meta)
            };
        }

        static CryMeta GetCryMeta(BinaryReader binaryReader, Endiness endiness)
        {
            return new()
            {
                NodeTableOffset = ReadBytesEndiness32(
                    binaryReader,
                    endiness),
                NodeTableCount = ReadBytesEndiness32(
                    binaryReader,
                    endiness),
                AttributeTableOffset = ReadBytesEndiness32(
                    binaryReader,
                    endiness),
                AttributeTableCount = ReadBytesEndiness32(
                    binaryReader,
                    endiness),
                ChildTableOffset = ReadBytesEndiness32(
                    binaryReader,
                    endiness),
                ChildTableCount = ReadBytesEndiness32(
                    binaryReader,
                    endiness),
                StringTableOffset = ReadBytesEndiness32(
                    binaryReader,
                    endiness),
                StringTableCount = ReadBytesEndiness32(
                    binaryReader,
                    endiness)
            };
        }

        bool CheckForCryHeader(
            BinaryReader binaryReader, 
            string header)
        {
            switch (header)
            {
                case "CryXml":
                case "CryXmlB":
                    SkipCString(binaryReader);
                    break;

                case "CRY3SDK":
                    binaryReader.ReadBytes(2);
                    break;

                default:
                    Log.LogError(
                        "Unknown File Format: {header})",
                        header);

                    return false;
            }

            return true;
        }

        string ReadHeader(char[] chars)
        {
            string header = new(chars);

            for (int i = 0; i < _charsLength; i++)
            {
                if (chars[i] == 0)
                {
                    header = new string(chars, 0, i);
                }
            }

            return header;
        }

        static int ReadBytesEndiness32(
            BinaryReader binaryReader,
            Endiness endiness)
        {
            var bytes = ReadBytesForInt(binaryReader);

            if (endiness == Endiness.Litle)
            {
                Array.Reverse(bytes);
            }

            int converted = BitConverter.ToInt32(bytes, 0);

            return converted;
        }

        static short ReadBytesEndiness16(
            BinaryReader binaryReader,
            Endiness endiness)
        {
            byte[] bytes = ReadBytesForShort(binaryReader);

            if (endiness == Endiness.Litle)
            {
                Array.Reverse(bytes);
            }

            short converted = BitConverter.ToInt16(bytes, 0);

            return converted;
        }


        static Endiness DetermineByteOrder(
            BinaryReader binaryReader,
            long headerLength)
        {
            var endiness = Endiness.Big;

            var bytes = ReadBytesForInt(binaryReader);

            int fileLength = BitConverter.ToInt32(bytes, 0);

            if (fileLength != binaryReader.BaseStream.Length)
            {
                binaryReader.BaseStream.Seek(headerLength, SeekOrigin.Begin);

                endiness = Endiness.Litle;

                ReadBytesForInt(binaryReader);
            }

            return endiness;
        }

        XmlDocument BuildXmlDocument(CryTable table)
        {
            var xmlDoc = new XmlDocument();

            Dictionary<int, XmlElement> xmlMap = [];

            int attributeIndex = 0;

            if (table.CryNode?.Count == 0)
            {
                Log.LogCritical("No nodes found in CryTable.");

                throw new FormatException("No nodes found in CryTable.");
            }

            if (table.CryReference?.Count == 0)
            {
                Log.LogCritical("No references found in CryTable.");

                throw new FormatException("No references found in CryTable.");
            }

            foreach (var node in table.CryNode!)
            {
                string? nodeName = table.Map?[node.NodeNameOffset] ?? null;

                if (string.IsNullOrWhiteSpace(nodeName))
                {
                    Log.LogCritical("Node name is empty or null");

                    throw new FormatException("Node name is empty or null.");
                }

                XmlElement element = xmlDoc.CreateElement(nodeName);

                //XmlElement element = xmlDoc.CreateElement(dataMap?[node.NodeNameOffset]!);

                for (int i = 0; i < node.AttributeCount; i++)
                {
                    var attributeRef = table.CryReference![attributeIndex++];

                    if (table.Map?.ContainsKey(attributeRef.ValueOffset) == true)
                    {
                        element.SetAttribute(
                            table.Map?[attributeRef.NameOffset] ?? "",
                            table.Map?[attributeRef.ValueOffset] ?? "");
                    }
                    else
                    {
                        element.SetAttribute(
                            table.Map?[attributeRef.NameOffset] ?? "",
                            "UNKNOWN");
                    }
                }

                if (table.Map?.ContainsKey(node.ContentOffset) == true)
                {
                    if (!string.IsNullOrWhiteSpace(table.Map?[node.ContentOffset]))
                    {
                        element.AppendChild(xmlDoc.CreateCDataSection(table.Map?[node.ContentOffset] ?? ""));
                    }
                }
                else
                {
                    element.AppendChild(xmlDoc.CreateCDataSection("UNKNOWN"));
                }

                xmlMap[node.NodeID] = element;

                if (xmlMap.TryGetValue(
                    node.ParentNodeID,
                    out XmlElement? value))
                {
                    value.AppendChild(element);
                }
                else
                {
                    xmlDoc.AppendChild(element);
                }
            }

            return xmlDoc;
        }

        static List<CryData> ReadCryData(
            BinaryReader br,
            CryMeta metadata)
        {
            List<CryData> dataTable = [];

            br.BaseStream.Seek(metadata.StringTableOffset, SeekOrigin.Begin);

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                var position = br.BaseStream.Position;

                CryData value = new()
                {
                    Offset = (int)position - metadata.StringTableOffset,
                    Value = ReadCString(br),
                };

                dataTable.Add(value);
            }
            return dataTable;
        }

        static List<int> ReadCryParent(
            BinaryReader br,
            CryMeta meta,
            Endiness endiness)
        {
            List<int> parentTable = [];

            br.BaseStream.Seek(meta.ChildTableOffset, SeekOrigin.Begin);

            while (br.BaseStream.Position < meta.ChildTableOffset + meta.ChildTableCount * sizeof(int))
            {
                int parentID = ReadBytesEndiness32(
                    br,
                    endiness);
                parentTable.Add(parentID);
            }

            return parentTable;
        }

        static List<CryReference> ReadCryAttributes(
            BinaryReader br,
            CryMeta meta,
            Endiness endiness)
        {
            List<CryReference> attributeTable = [];

            br.BaseStream.Seek(meta.AttributeTableOffset, SeekOrigin.Begin);

            while (br.BaseStream.Position < meta.AttributeTableOffset + meta.AttributeTableCount * meta.ReferenceTableSize)
            {
                CryReference value = new()
                {
                    NameOffset = ReadBytesEndiness32(br, endiness),
                    ValueOffset = ReadBytesEndiness32(br, endiness)
                };

                attributeTable.Add(value);
            }

            return attributeTable;
        }

        static List<CryNode> ReadCryNode(
            BinaryReader br,
            CryMeta meta,
            Endiness endiness)
        {
            List<CryNode> nodeTable = [];

            br.BaseStream.Seek(meta.NodeTableOffset, SeekOrigin.Begin);

            int nodeID = 0;

            while (br.BaseStream.Position < meta.NodeTableOffset + meta.NodeTableCount * meta.NodeTableSize)
            {
                CryNode value = new()
                {
                    NodeID = nodeID++,
                    NodeNameOffset = ReadBytesEndiness32(
                        br,
                        endiness),
                    ContentOffset = ReadBytesEndiness32(
                        br,
                        endiness),
                    AttributeCount = ReadBytesEndiness16(
                        br,
                        endiness),
                    ChildCount = ReadBytesEndiness16(
                        br,
                        endiness),
                    ParentNodeID = ReadBytesEndiness32(
                        br,
                        endiness),
                    FirstAttributeIndex = ReadBytesEndiness32(
                        br,
                        endiness),
                    FirstChildIndex = ReadBytesEndiness32(
                        br,
                        endiness),
                    Reserved = ReadBytesEndiness32(
                        br,
                        endiness),
                };

                nodeTable.Add(value);
            }

            return nodeTable;
        }

        static void SkipCString(BinaryReader binaryReader)
        {
            ArgumentNullException.ThrowIfNull(binaryReader);

            // read characters one by one until the null terminator is found.
            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length &&
                (_ = binaryReader.ReadChar()) != '\0')
            {
                // do nothing inside the loop, just keep reading to advance the stream position.
                // TODO: something more elegant than this...
            }

            // the BinaryReader is now positioned right after the null terminator, ready for further reading
        }

        static string? ReadCString(BinaryReader binaryReader)
        {
            ArgumentNullException.ThrowIfNull(binaryReader);

            StringBuilder result = new();

            char ch;

            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length &&
                (ch = binaryReader.ReadChar()) != '\0')
            {
                result.Append(ch);
            }

            // after reading the null terminator, we're at the correct position to continue reading the stream,
            // so there's no need to seek back or forward
            return result.Length > 0 ? result.ToString() : null;
        }

        static byte[] ReadBytesForInt(BinaryReader binaryReader)
        {
            return
            [
                binaryReader.ReadByte(),
                binaryReader.ReadByte(),
                binaryReader.ReadByte(),
                binaryReader.ReadByte(),
            ];
        }

        static byte[] ReadBytesForShort(BinaryReader binaryReader)
        {
            return
            [
                binaryReader.ReadByte(),
                binaryReader.ReadByte()
            ];
        }
    }
}
