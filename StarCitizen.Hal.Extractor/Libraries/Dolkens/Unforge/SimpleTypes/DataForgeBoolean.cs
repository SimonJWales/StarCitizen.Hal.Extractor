
using System.Xml;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.SimpleTypes
{
    public class DataForgeBoolean : DataForgeSerializable
    {
        public bool Value { get; set; }

        public DataForgeBoolean(DataForge documentRoot)
            : base(documentRoot)
        {
            Value = _br.ReadBoolean();
        }

        public override string ToString()
        {
            return string.Format("{0}", Value ? "1" : "0");
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("Bool");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value ? "1" : "0";

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
