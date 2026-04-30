namespace Intent.Contract.Models
{
    /// <summary>
    /// Records whether the WallIntent location curve was provided
    /// directly by the user or derived from a brep by the system.
    /// </summary>
    public enum GeometrySource
    {
        Unknown = 0,
        Curve = 1,
        Brep = 2,
        Extrusion = 3
    }
}