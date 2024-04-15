using System.Xml;

namespace unforge
{
    public class DataForgeUInt16 : _DataForgeSerializable
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
