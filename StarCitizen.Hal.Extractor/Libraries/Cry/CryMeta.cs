
namespace StarCitizen.Hal.Extractor.Library.Cry
{
    public class CryMeta
    {
        public int NodeTableOffset { get; set; }
        public int NodeTableCount { get; set; }
        public int AttributeTableOffset { get; set; }
        public int AttributeTableCount { get; set; }
        public int ChildTableOffset { get; set; }
        public int ChildTableCount { get; set; }
        public int StringTableOffset { get; set; }
        public int StringTableCount { get; set; }
        public int NodeTableSize { get; set; } = 28;
        public int ReferenceTableSize { get; set; } = 8;
        public int Length3 { get; set; } = 4;
    }
}
