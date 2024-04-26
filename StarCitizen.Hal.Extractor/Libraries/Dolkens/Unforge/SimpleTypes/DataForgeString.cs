using System;

namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge.SimpleTypes
{
    public class DataForgeString : DataForgeSerializable
    {
        public string Value { get; set; }

        public DataForgeString(DataForge documentRoot)
            : base(documentRoot) => Value = _br.ReadCString()!;

        public override string ToString()
        {
            return Value;
        }
    }
}
