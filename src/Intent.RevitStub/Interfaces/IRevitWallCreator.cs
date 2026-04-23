using Intent.RevitStub.Models;

namespace Intent.RevitStub.Interfaces
{
    /// <summary>
    /// Defines the contract for placing a wall in a Revit document.
    /// 
    /// Responsibilities:
    /// - Accept a RevitMappingResult and place a wall element
    /// - REsolve the wall type ELementID from the type name
    /// - Apply all parameters
    /// - Return a RevitCreationResult indicating success or failure
    /// </summary>
    public interface IRevitWallCreator
    {
        /// <summary>
        /// Places a wall in the active Revit document using the
        /// parameters carried by the mapping result.
        /// </summary>
        /// <param name="mappingResult">Revit-ready parameters produced by IWallIntentMapper</param>
        /// <returns>
        /// A RevitCreationResult Indicating whether the wall was
        /// created successfully, and carrying the new ElementID if so.
        /// </returns>
        RevitCreationResult Create(RevitMappingResult mappingResult);
    }
}