using System.Xml;

namespace unforge
{
    public class DataForgeStringLookup : _DataForgeSerializable
    {
        uint _value;

        public string Value { get { return DocumentRoot.ValueMap[_value]; } }

        public DataForgeStringLookup(DataForge documentRoot)
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
            var element = DocumentRoot.CreateElement("String");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value;

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
