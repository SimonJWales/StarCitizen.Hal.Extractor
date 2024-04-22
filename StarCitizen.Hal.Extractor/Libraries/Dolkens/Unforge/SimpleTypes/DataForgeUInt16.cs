using System.Xml;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.SimpleTypes
{
    public class DataForgeUInt16 : DataForgeSerializable
    {
        public ushort Value { get; set; }

        public DataForgeUInt16(DataForge documentRoot)
            : base(documentRoot)
        {
            Value = _br.ReadUInt16();
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("UInt16");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
