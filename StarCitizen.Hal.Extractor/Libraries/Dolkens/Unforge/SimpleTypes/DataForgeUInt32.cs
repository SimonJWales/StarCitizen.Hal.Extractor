using System.Xml;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.SimpleTypes
{
    public class DataForgeUInt32 : DataForgeSerializable
    {
        public uint Value { get; set; }

        public DataForgeUInt32(DataForge documentRoot)
            : base(documentRoot)
        {
            Value = _br.ReadUInt32();
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("UInt32");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
