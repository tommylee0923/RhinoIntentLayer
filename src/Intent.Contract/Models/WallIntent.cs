using System.Collections.Generic;

namespace Intent.Contract.Models
{
    /// <summary>
    /// Structured intent for a Revit wall element.
    /// All properties map directly to Revit wwall instance or 
    /// type parameters.
    /// 
    /// Instance properties:
    ///     UnconnectedHeight, BaaseOffset, TopOffset, LocationLine,
    ///     IsStructural
    /// 
    /// Type-level hints:
    ///     TypeName, NominalWidth
    /// </summary>
    public sealed class WallIntent
    {
        // ----------------------------------------------------------
        // Schema / Identity
        // ----------------------------------------------------------
        public string SchemaVersion { get; set; }
        public string StableId { get; set; }

        // Must be ObjectType.Wall
        public ObjectType ObjectType { get; set; }

        // ----------------------------------------------------------
        // Type-level hints
        // ----------------------------------------------------------
        public string TypeName { get; set; }
        // Nominal wall thickness in meters.
        public double? NominalWidth {get; set; }

        // ----------------------------------------------------------
        // Instance properties
        // ----------------------------------------------------------
        public double? UnconnectedHeight { get; set; }
        public double? BaseOffset {get; set; }
        public double? TopOffset {get; set; }
        public LocationLine? LocationLine {get; set; }
        public bool? IsStructural {get; set; }
        
        // ----------------------------------------------------------
        // Extensibilitiy
        // ----------------------------------------------------------
        // Arbitrary key-value pairs for future properties without
        // requiring a schema version bump
        public Dictionary<string, string> Extensions { get; set; }

        public WallIntent()
        {
            SchemaVersion = string.Empty;
            StableId = string.Empty;
            ObjectType = ObjectType.Unknown;
            TypeName = string.Empty;
            LocationLine = Models.LocationLine.WallCenterline;
            BaseOffset = 0.0;
            TopOffset = 0.0;
            Extensions = new Dictionary<string, string>();
        }
    }
}