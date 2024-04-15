using System.Xml;

namespace unforge
{
    public class DataForgeEnum : _DataForgeSerializable
    {
        uint _value;

        public string Value { get { return DocumentRoot.ValueMap[_value]; } }

        public DataForgeEnum(DataForge documentRoot)
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
            var element = DocumentRoot.CreateElement("Enum");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value;

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
