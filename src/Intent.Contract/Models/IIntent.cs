namespace Intent.Contract.Models
{
    /// <summary>
    /// Base contract for all intent DTOs.
    /// 
    /// Contains only the field that IntentService needs to access generically
    /// across all intent types.
    /// </summary>
    public interface IIntent
    {
        string SchemaVersion {get; set; }
        string StableId {get; set; }
        GeometrySource GeometrySource {get; set; }
    }
}