namespace Knapcode.NuGetProtocol.V2
{
    public class EntityProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Nullable { get; set; }
        public string TargetPath { get; set; }
        public string ContentKind { get; set; }
        public bool KeepInContent { get; set; }
    }
}
