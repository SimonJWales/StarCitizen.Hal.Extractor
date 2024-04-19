
namespace StarCitizen.Hal.Extractor.Library.Cry
{
    public class CryTable
    {
        public List<CryNode>? CryNode { get; set; }
        public List<CryData>? CryData { get; set; }
        public List<CryReference>? CryReference { get; set; }
        public List<int>? Parent { get; set; }
        public Dictionary<int, string?>? Map { get; set; }
    }
}
