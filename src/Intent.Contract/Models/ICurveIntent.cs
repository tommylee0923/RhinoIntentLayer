namespace Intent.Contract.Models
{
    /// <summary>
    /// Extended contract for curve-driven intent types.
    /// Applies to walls, floows, beams - any element defined by a location line.
    /// </summary>
    public interface ICurveIntent : IIntent
    {
        double[] LocationCurveStart {get; set; }
        double[] LocationCurveEnd {get; set; }
    }
}