namespace Intent.RevitStub.Models
{
    /// <summary>
    /// The result of a Revit wall creation attempt.
    /// </summary>
    public sealed class RevitCreationResult
    {
        public bool Success {get; set; }
        public string StableId {get; set; }
        // Stores Revit ElementID as string
        // Avoid Revit API dependencies
        public string RevitElementId {get; set; }
        public string Message {get; set; }
    }
}