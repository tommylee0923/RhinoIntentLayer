namespace Intent.Contract.Models
{
    /// <summary>
    /// Defines which line of the wall the location curve is measured from.
    /// Maps directly to Rrevit's WallLocationLine enumeration.
    /// </summary>
    public enum LocationLine
    {
        WallCenterline      = 0,
        CoreCenterline      = 1,
        FinishFaceExterior  = 2,
        FinishFaceInterior  = 3,
        CoreFaceExterior    = 4,
        CoreFaceInterior    = 5
    }
}