using System.Xml;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.SimpleTypes
{
    public class DataForgeSingle : DataForgeSerializable
    {
        public float Value { get; set; }

        public DataForgeSingle(DataForge documentRoot)
            : base(documentRoot)
        {
            Value = _br.ReadSingle();
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("Single");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
