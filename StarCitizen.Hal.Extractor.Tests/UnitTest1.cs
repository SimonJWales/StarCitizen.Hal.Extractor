
using System.Data.Common;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using StarCitizen.Hal.Extractor;
using StarCitizen.Hal.Extractor.Library.Cry;
using StarCitizen.Hal.Extractor.Library.Enum;

namespace StarCitizen.Hal.Extractor.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var file = @"E:\_SCB\Unknown\LIVE_3.22.138.53555\Data\Scripts\Entities\Vehicles\Implementations\Xml\AEGS_Avenger.xml";

            using FileStream fs = new(
                file, 
                FileMode.Open);

            using BinaryReader binaryReader = new(fs);

            int peek = binaryReader.PeekChar();

            bool isXML = peek == '<' || peek == 65279;

            if (isXML)
            {
                // already an xml file (or not anything we want to decrpyt)
                return;
            }

            if (peek != 'C')
            {
                throw new FormatException($"Invalid CryXML file (peek: {peek})");
            }

            // read a Fixed-Length string from the stream
            char[] chars = binaryReader.ReadChars(7);

            string header = new(chars);

            for (int i = 0; i < 7; i++)
            {
                if (chars[i] == 0)
                {
                    header = new string(chars, 0, i);
                }
            }

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
                    throw new FormatException($"Unknown File Format: {header}");
            }

            long headerLength = binaryReader.BaseStream.Position;

            Endiness endiness = DetermineByteOrder(binaryReader, headerLength);

            CryMeta meta = new()
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

            CryTable table = new()
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

            table.Map = table.CryData
                .ToDictionary(k => k.Offset, v => v.Value);

            fs.Close(); // close the underlying stream, so we can save back to the same file

            XmlDocument xml = BuildXmlDocument(table);

            xml?.Save(file);
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

        private static short ReadBytesEndiness16(
            BinaryReader binaryReader, 
            Endiness endiness)
        {
            byte[] bytes = [
                binaryReader.ReadByte(),
                binaryReader.ReadByte()
            ];

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

        static XmlDocument BuildXmlDocument(CryTable table)
        {
            var xmlDoc = new XmlDocument();

            Dictionary<int, XmlElement> xmlMap = [];

            int attributeIndex = 0;

            if (table.CryNode?.Count == 0)
            {
                throw new FormatException("No nodes found in CryTable.");
            }

            if (table.CryReference?.Count == 0)
            {
                throw new FormatException("No references found in CryTable.");
            }

            foreach (var node in table.CryNode!)
            {
                string? nodeName = table.Map?[node.NodeNameOffset] ?? null;

                if (string.IsNullOrWhiteSpace(nodeName))
                {
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
                            table.Map? [attributeRef.NameOffset] ?? "",
                            "BUGGED");
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
                    element.AppendChild(xmlDoc.CreateCDataSection("BUGGED"));
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