
namespace StarCitizen.Hal.Extractor.Library.Dolkens.Unforge
{
    public abstract class DataForgeSerializable(DataForge documentRoot)
    {
        internal DataForge DocumentRoot { get; set; } = documentRoot;

        internal BinaryReader _br = documentRoot._br;
    }
}
