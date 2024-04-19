﻿
namespace StarCitizen.Hal.Extractor.Library.Cry
{
    public class CryNode
    {
        public int NodeID { get; set; }
        public int NodeNameOffset { get; set; }
        public int ContentOffset { get; set; }
        public short AttributeCount { get; set; }
        public short ChildCount { get; set; }
        public int ParentNodeID { get; set; }
        public int FirstAttributeIndex { get; set; }
        public int FirstChildIndex { get; set; }
        public int Reserved { get; set; }
    }
}
