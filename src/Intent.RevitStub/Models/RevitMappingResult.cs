using System.Runtime.CompilerServices;

namespace Intent.RevitStub.Models
{
    /// <summary>
    /// The output of mapping a WallIntent to Revit-ready parameters.
    /// </summary>
    public sealed class RevitMappingResult
    {
        public string StableId {get; set; }
        public string ResolvedTypeName {get; set; }
        public double HeightInternalUnits {get; set; }
        public double BassOffsetInternalUnits {get; set; }
        public double TopOffsetInternalUnits {get; set; }
        public int LocationLineValue {get; set; }
        public bool IsStructural {get; set; }
        public double NominalWidthInternalUnits {get; set; }
    }
}