using System.Xml;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.SimpleTypes
{
    public class DataForgeInt8 : DataForgeSerializable
    {
        public sbyte Value { get; set; }

        public DataForgeInt8(DataForge documentRoot)
            : base(documentRoot)
        {
            Value = _br.ReadSByte();
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("Int8");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
