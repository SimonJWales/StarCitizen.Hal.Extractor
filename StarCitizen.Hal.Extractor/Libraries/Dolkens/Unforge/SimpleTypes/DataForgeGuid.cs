using System;
using System.IO;
using System.Xml;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.SimpleTypes
{
    public class DataForgeGuid : DataForgeSerializable
    {
        public Guid Value { get; set; }

        public DataForgeGuid(DataForge documentRoot)
            : base(documentRoot) => Value = _br.ReadGuid(false)!.Value;

        public override string ToString()
        {
            return Value.ToString();
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("Guid");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
