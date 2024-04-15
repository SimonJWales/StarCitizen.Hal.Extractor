using System.IO;

namespace unforge
{
    public abstract class _DataForgeSerializable
    {
        internal DataForge DocumentRoot { get; set; }

        internal BinaryReader _br;
        
        public _DataForgeSerializable(DataForge documentRoot)
        {
            DocumentRoot = documentRoot;
            _br = documentRoot._br;
        }
    }
}
