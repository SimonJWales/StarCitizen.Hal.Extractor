using System.Xml;

namespace unforge
{
    public class DataForgeLocale : _DataForgeSerializable
    {
        uint _value;

        public string Value { get { return DocumentRoot.ValueMap[_value]; } }

        public DataForgeLocale(DataForge documentRoot)
            : base(documentRoot)
        {
            _value = _br.ReadUInt32();
        }

        public override string ToString()
        {
            return Value;
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("LocID");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            // TODO: More work here
            element.Attributes.Append(attribute);

            return element;
        }
    }
}
