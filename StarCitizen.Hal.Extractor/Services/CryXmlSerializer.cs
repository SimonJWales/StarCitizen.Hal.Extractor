﻿
using StarCitizen.Hal.Extractor.Entities.CryXmlB;
using System.Xml;
using System.Xml.Serialization;

namespace Hal.Extractor.Services;

public enum ByteOrderEnum
{
    AutoDetect,
    BigEndian,
    LittleEndian,
}

public static class CryXmlSerializer
{
    public static int ReadInt32(
        this BinaryReader br,
        ByteOrderEnum byteOrder = ByteOrderEnum.BigEndian)
    {
        var bytes = new byte[] {
            br.ReadByte(),
            br.ReadByte(),
            br.ReadByte(),
            br.ReadByte(),
        };

        if (byteOrder == ByteOrderEnum.LittleEndian)
            bytes = bytes.Reverse().ToArray();

        return BitConverter.ToInt32(bytes, 0);
    }

    public static short ReadInt16(
        this BinaryReader br,
        ByteOrderEnum byteOrder = ByteOrderEnum.BigEndian)
    {
        var bytes = new byte[] {
            br.ReadByte(),
            br.ReadByte(),
        };

        if (byteOrder == ByteOrderEnum.LittleEndian)
            bytes = bytes.Reverse().ToArray();

        return BitConverter.ToInt16(bytes, 0);
    }

    public static XmlDocument ReadFile(
        string inFile,
        ByteOrderEnum byteOrder = ByteOrderEnum.AutoDetect)
    {
        return ReadStream(File.OpenRead(inFile), byteOrder)!;
    }

    public static XmlDocument? ReadStream(
        Stream inStream,
        ByteOrderEnum byteOrder = ByteOrderEnum.AutoDetect)
    {
        using BinaryReader br = new BinaryReader(inStream);

        int peek = br.PeekChar();

        if (IsXmlOrBOM(peek))
        {
            return null;
        }

        ValidateFirstCharacter(peek);

        string header = br.ReadFString(7)!;

        HandleExtraData(
            br,
            header);

        ValidateHeader(header);

        long headerLength = br.BaseStream.Position;

        byteOrder = DetermineByteOrder(
            br,
            headerLength);

        CryXMLContentMetaData metadata = ReadMetadata(
            br,
            byteOrder);

        List<CryXmlNode> nodeTable = ReadNodeTable(
            br,
            byteOrder,
            metadata);

        List<CryXmlReference> attributeTable = ReadAttributeTable(
            br,
            byteOrder,
            metadata);

        List<int> parentTable = ReadParentTable(
            br,
            byteOrder,
            metadata);

        List<CryXmlValue> dataTable = ReadDataTable(
            br,
            metadata);

        Dictionary<int, string?>? dataMap = dataTable
            .ToDictionary(k => k.Offset, v => v.Value);

        return BuildXmlDocument(
            nodeTable,
            attributeTable,
            dataMap);
    }

    static bool IsXmlOrBOM(int peek)
    {
        return peek == '<' || peek == 65279;
    }

    static void ValidateFirstCharacter(int peek)
    {
        if (peek != 'C')
        {
            throw new FormatException($"Unknown File Format (PeekChar: {peek})");
        }
    }

    static void ValidateHeader(string header)
    {
        if (header != "CryXml" &&
            header != "CryXmlB" &&
            header != "CRY3SDK")
        {
            throw new FormatException($"Unknown File Format: {header}");
        }
    }

    static void HandleExtraData(
        BinaryReader br,
        string header)
    {
        if (header == "CryXml" || header == "CryXmlB")
        {
            br.ReadCString();
        }
        else if (header == "CRY3SDK")
        {
            _ = br.ReadBytes(2);
        }
    }

    static ByteOrderEnum DetermineByteOrder(
        BinaryReader br,
        long headerLength)
    {
        ByteOrderEnum byteOrder = ByteOrderEnum.BigEndian;

        int fileLength = br.ReadInt32(byteOrder);

        if (fileLength != br.BaseStream.Length)
        {
            br.BaseStream.Seek(headerLength, SeekOrigin.Begin);

            byteOrder = ByteOrderEnum.LittleEndian;

            _ = br.ReadInt32(byteOrder);
        }

        return byteOrder;
    }

    static CryXMLContentMetaData ReadMetadata(
        BinaryReader br,
        ByteOrderEnum byteOrder)
    {
        return new CryXMLContentMetaData(
            br.ReadInt32(byteOrder), // NodeTableOffset
            br.ReadInt32(byteOrder), // NodeTableCount
            br.ReadInt32(byteOrder), // AttributeTableOffset
            br.ReadInt32(byteOrder), // AttributeTableCount
            br.ReadInt32(byteOrder), // ChildTableOffset
            br.ReadInt32(byteOrder), // ChildTableCount
            br.ReadInt32(byteOrder), // StringTableOffset
            br.ReadInt32(byteOrder)  // StringTableCount
        );
    }
    static List<CryXmlNode> ReadNodeTable(
        BinaryReader br,
        ByteOrderEnum byteOrder,
        CryXMLContentMetaData metadata)
    {
        List<CryXmlNode> nodeTable = new List<CryXmlNode> { };

        br.BaseStream.Seek(metadata.NodeTableOffset, SeekOrigin.Begin);

        int nodeID = 0;

        while (br.BaseStream.Position < metadata.NodeTableOffset + metadata.NodeTableCount * metadata.NodeTableSize)
        {
            var value = new CryXmlNode
            {
                NodeID = nodeID++,
                NodeNameOffset = br.ReadInt32(byteOrder),
                ContentOffset = br.ReadInt32(byteOrder),
                AttributeCount = br.ReadInt16(byteOrder),
                ChildCount = br.ReadInt16(byteOrder),
                ParentNodeID = br.ReadInt32(byteOrder),
                FirstAttributeIndex = br.ReadInt32(byteOrder),
                FirstChildIndex = br.ReadInt32(byteOrder),
                Reserved = br.ReadInt32(byteOrder),
            };

            nodeTable.Add(value);
        }

        return nodeTable;
    }

    static List<CryXmlReference> ReadAttributeTable(
        BinaryReader br,
        ByteOrderEnum byteOrder,
        CryXMLContentMetaData metadata)
    {
        List<CryXmlReference> attributeTable = new List<CryXmlReference> { };

        br.BaseStream.Seek(metadata.AttributeTableOffset, SeekOrigin.Begin);

        while (br.BaseStream.Position < metadata.AttributeTableOffset + metadata.AttributeTableCount * metadata.ReferenceTableSize)
        {
            var value = new CryXmlReference
            {
                NameOffset = br.ReadInt32(byteOrder),
                ValueOffset = br.ReadInt32(byteOrder)
            };

            attributeTable.Add(value);
        }

        return attributeTable;
    }

    static List<int> ReadParentTable(
        BinaryReader br,
        ByteOrderEnum byteOrder,
        CryXMLContentMetaData metadata)
    {
        List<int> parentTable = new List<int>();

        br.BaseStream.Seek(metadata.ChildTableOffset, SeekOrigin.Begin);

        while (br.BaseStream.Position < metadata.ChildTableOffset + metadata.ChildTableCount * sizeof(int))
        {
            int parentID = br.ReadInt32(byteOrder);
            parentTable.Add(parentID);
        }

        return parentTable;
    }

    static List<CryXmlValue> ReadDataTable(
        BinaryReader br,
        CryXMLContentMetaData metadata)
    {
        List<CryXmlValue> dataTable = new List<CryXmlValue> { };

        br.BaseStream.Seek(metadata.StringTableOffset, SeekOrigin.Begin);

        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var position = br.BaseStream.Position;

            var value = new CryXmlValue
            {
                Offset = (int)position - metadata.StringTableOffset,
                Value = br.ReadCString(),
            };

            dataTable.Add(value);
        }
        return dataTable;
    }

    static XmlDocument BuildXmlDocument(
        List<CryXmlNode> nodeTable,
        List<CryXmlReference> attributeTable,
        Dictionary<int, string?>? dataMap)
    {
        var xmlDoc = new XmlDocument();

        Dictionary<int, XmlElement> xmlMap = new Dictionary<int, XmlElement> { };

        int attributeIndex = 0;

        foreach (var node in nodeTable)
        {
            XmlElement element = xmlDoc.CreateElement(dataMap?[node.NodeNameOffset]!);

            for (int i = 0; i < node.AttributeCount; i++)
            {
                var attributeRef = attributeTable[attributeIndex++];

                if (dataMap?.ContainsKey(attributeRef.ValueOffset) == true)
                {
                    element.SetAttribute(
                        dataMap[attributeRef.NameOffset] ?? "",
                        dataMap[attributeRef.ValueOffset] ?? "");
                }
                else
                {
                    element.SetAttribute(
                        dataMap?[attributeRef.NameOffset] ?? "",
                        "BUGGED");
                }
            }

            if (dataMap?.ContainsKey(node.ContentOffset) == true)
            {
                if (!string.IsNullOrWhiteSpace(dataMap[node.ContentOffset]))
                {
                    element.AppendChild(xmlDoc.CreateCDataSection(dataMap[node.ContentOffset] ?? ""));
                }
            }
            else
            {
                element.AppendChild(xmlDoc.CreateCDataSection("BUGGED"));
            }

            xmlMap[node.NodeID] = element;

            if (xmlMap.ContainsKey(node.ParentNodeID))
            {
                xmlMap[node.ParentNodeID].AppendChild(element);
            }
            else
            {
                xmlDoc.AppendChild(element);
            }
        }

        return xmlDoc;
    }

    public static TObject? Deserialize<TObject>(
        string inFile,
        ByteOrderEnum byteOrder = ByteOrderEnum.BigEndian) where TObject : class
    {
        using MemoryStream ms = new MemoryStream();
        var xmlDoc = ReadFile(inFile, byteOrder);

        xmlDoc.Save(ms);

        ms.Seek(0, SeekOrigin.Begin);

        XmlSerializer xs = new XmlSerializer(typeof(TObject));

        return xs.Deserialize(ms) as TObject;
    }
}
