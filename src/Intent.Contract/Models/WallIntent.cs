using System.Collections.Generic;

namespace Intent.Contract.Models
{
    public sealed class WallIntent
    {
        public string SchemaVersion { get; set; }
        public string StableId { get; set; }
        public ObjectType ObjectType { get; set; }
        public string TypeName { get; set; }
        public double? UnconnectedHeight { get; set; }
        public Dictionary<string, string> Extensions { get; set; }

        public WallIntent()
        {
            SchemaVersion = string.Empty;
            StableId = string.Empty;
            ObjectType = ObjectType.Unknown;
            TypeName = string.Empty;
            Extensions = new Dictionary<string, string>();
        }
    }
}