namespace StarCitizen.Hal.Extractor.Entities.CryXmlB
{
    internal class CryXMLContentMetaData
    {
        public int NodeTableOffset { get; set; }
        public int NodeTableCount { get; set; }
        public int AttributeTableOffset { get; set; }
        public int AttributeTableCount { get; set; }
        public int ChildTableOffset { get; set; }
        public int ChildTableCount { get; set; }
        public int StringTableOffset { get; set; }
        public int StringTableCount { get; set; }
        public int NodeTableSize { get; set; }
        public int ReferenceTableSize { get; set; }
        public int Length3 { get; set; }

        public CryXMLContentMetaData(
            int nodeTableOffset,
            int nodeTableCount,
            int attributeTableOffset,
            int attributeTableCount,
            int childTableOffset,
            int childTableCount,
            int stringTableOffset,
            int stringTableCount)
        {
            NodeTableOffset = nodeTableOffset;
            NodeTableCount = nodeTableCount;
            AttributeTableOffset = attributeTableOffset;
            AttributeTableCount = attributeTableCount;
            ChildTableOffset = childTableOffset;
            ChildTableCount = childTableCount;
            StringTableOffset = stringTableOffset;
            StringTableCount = stringTableCount;
            NodeTableSize = 28;
            ReferenceTableSize = 8;
            Length3 = 4;

        }
    }
}
