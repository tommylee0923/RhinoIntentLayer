using Intent.Contract.Models;
using Intent.RevitStub.Models;

namespace Intent.RevitStub.Interfaces
{
    /// <summary>
    /// Defines the contract for mapping a validated WallIntent to
    /// Revit-ready parameters.
    /// 
    /// Responsibilites:
    /// - Convert domain units (meters) to Revit internal units (feet)
    /// - Resolve TypeName to a known Revit wall type
    /// - Map LocationLine enum to Revit's WallLocationLine integer value
    /// </summary>
    public interface IWallIntentMapper
    {
        /// <summary>
        /// Maps a validated WallIntent to a RevitMappingResult.
        /// Callers should only pass intents where ValidationResult.IsValid
        /// is true.
        /// </summary>
        /// <param name="intent">A validated Wallintent</param>
        /// <returns>
        /// A RevitMappingResult carrying Revit-ready parameters,
        /// or null if the intent cannot be mapped.
        /// </returns>
        RevitMappingResult Map(WallIntent intent);
    }
}