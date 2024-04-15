using System.Xml;

namespace unforge
{
    public class DataForgeInt64 : _DataForgeSerializable
    {
        public long Value { get; set; }

        public DataForgeInt64(DataForge documentRoot)
            : base(documentRoot)
        {
            Value = _br.ReadInt64();
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("Int64");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
