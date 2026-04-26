using Intent.Contract.Models;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Intent.RhinoLayer
{
    /// <summary>
    /// Derives a wall location line from a selected RhinoObject.
    /// 
    /// Supports two input types:
    ///     Curve - Returned directly if linear, approximated if not.
    ///     Brep - Centerline extracted from the bottom face
    /// 
    /// Returns null if extraction fails.
    /// </summary>
    internal static class WallGeometryExtractor
    {
        /// <summary>
        /// Attempts to a wall centerline from the given object.
        /// Returns null if the geometry type is unsupported or extraction fails.
        /// </summary>
        /// <returns></returns>
        public static LineCurve TryExtract(RhinoObject rhinoObject, out GeometrySourceResult source)
        {
            source = GeometrySourceResult.Unknown;

            if (rhinoObject.Geometry is Curve curve)
            {
                return ExtractFromCurve(curve, out source);
            }

            if (rhinoObject.Geometry is Brep brep)
            {
                return ExtractFromBrep(brep, out source);
            }

            return null;
        }
        // ----------------------------------------------------------
        // Curve Path
        // ----------------------------------------------------------
        private static LineCurve ExtractFromCurve(Curve curve, out GeometrySourceResult source)
        {
            source = GeometrySourceResult.Curve;

            if (curve is LineCurve lineCurve)
            {
                return lineCurve;
            }

            if (curve.IsLinear())
            {
                return new LineCurve(curve.PointAtStart, curve.PointAtEnd);
            }

            source = GeometrySourceResult.CurveApproximated;
            return new LineCurve(curve.PointAtStart, curve.PointAtEnd);
        }

        // ----------------------------------------------------------
        // Brep Path
        // ----------------------------------------------------------
        private static LineCurve ExtractFromBrep(Brep brep, out GeometrySourceResult source)
        {
            source = GeometrySourceResult.Unknown;

            var bbox = brep.GetBoundingBox(accurate: true);
            if (!bbox.IsValid)
            {
                return null;
            }

            double baseZ = bbox.Min.Z;

            BrepFace bottomFace = null;
            double closestZ = double.MaxValue;

            foreach (var face in brep.Faces)
            {
                var facebox = face.GetBoundingBox(accurate: true);
                double faceZ = facebox.Center.Z;

                if (faceZ < closestZ)
                {
                    closestZ = faceZ;
                    bottomFace = face;
                }
            }

            if (bottomFace == null)
                return null;

            BrepEdge longestEdge = null;
            double longestLength = 0;

            foreach (var edgeIndex in bottomFace.AdjacentEdges())
            {
                var edge = brep.Edges[edgeIndex];
                double length = edge.GetLength();

                if (length > longestLength)
                {
                    longestLength = length;
                    longestEdge = edge;
                }
            }

            if (longestEdge == null)
                return null;

            var edgeStart = longestEdge.PointAtStart;
            var edgeEnd = longestEdge.PointAtEnd;

            var start = new Point3d(edgeStart.X, edgeStart.Y, baseZ);
            var end = new Point3d(edgeEnd.X, edgeEnd.Y, baseZ);

            var faceCenter = bottomFace.GetBoundingBox(accurate: true).Center;
            var edgeDir = (end - start);
            edgeDir.Unitize();
            var perpDir = Vector3d.CrossProduct(edgeDir, Vector3d.ZAxis);
            perpDir.Unitize();

            var toCenter = faceCenter - start;
            double perpDist = toCenter * perpDir;
            var offset = perpDir * perpDist;

            start += offset;
            end += offset;

            if (start.DistanceTo(end) < Rhino.RhinoMath.ZeroTolerance)
                return null;

            source = GeometrySourceResult.Brep;
            return new LineCurve(start, end);
        }
    };

    internal enum GeometrySourceResult
    {
        Unknown,
        Curve,
        CurveApproximated,
        Brep
    }
}