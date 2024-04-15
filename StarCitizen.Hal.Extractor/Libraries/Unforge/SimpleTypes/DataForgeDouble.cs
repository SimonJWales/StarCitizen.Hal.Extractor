﻿using System.Xml;

namespace unforge
{
    public class DataForgeDouble : _DataForgeSerializable
    {
        public double Value { get; set; }

        public DataForgeDouble(DataForge documentRoot)
            : base(documentRoot)
        {
            Value = _br.ReadDouble();
        }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("Double");

            var attribute = DocumentRoot.CreateAttribute("value");

            attribute.Value = Value.ToString();

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
