﻿using System;
using System.IO;
using System.Xml;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.SimpleTypes
{
    public class DataForgeReference : DataForgeSerializable
    {
        public uint Item1 { get; set; }

        public Guid Value { get; set; }

        public DataForgeReference(DataForge documentRoot)
            : base(documentRoot)
        {
            Item1 = _br.ReadUInt32();

            Value = _br.ReadGuid(false)!.Value;
        }

        public override string ToString()
        {
            return string.Format("0x{0:X8} 0x{1}", Item1, Value);
        }

        public XmlElement Read()
        {
            var element = DocumentRoot.CreateElement("Reference");

            var attribute = DocumentRoot.CreateAttribute("value");

            // TODO: More work here
            attribute.Value = $"{Value}";

            element.Attributes.Append(attribute);

            return element;
        }
    }
}
