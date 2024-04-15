using System.Xml;

namespace unforge
{
    public class DataForgeUInt64 : _DataForgeSerializable
    {
        public ulong Value { get; set; }

        public DataForgeUInt64(DataForge documentRoot)
            : base(documentRoot)
        {
            Value = _br.ReadUInt64();
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("UInt64");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
