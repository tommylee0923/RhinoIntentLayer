using System.Data;
using Eto.Forms;
using Intent.Contract.Models;
using Rhino;
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

            if (rhinoObject.Geometry is Extrusion extrusion)
            {
                return ExtractFromExtrusion(extrusion, out source);
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

        // ----------------------------------------------------------
        // Extrusion Path
        // ----------------------------------------------------------
        private static LineCurve ExtractFromExtrusion(Extrusion extrusion, out GeometrySourceResult source)
        {
            source = GeometrySourceResult.Unknown;

            var profile = extrusion.Profile3d(0, 0);
            if (profile == null)
                return null;
            
            var path = extrusion.PathLineCurve();
            if (path == null)
                return null;
            
            double baseZ = path.PointAtStart.Z;

            // Open extrusion from a line
            if (profile.IsLinear())
            {
                var start = new Point3d(profile.PointAtStart.X, profile.PointAtStart.Y, baseZ);
                var end = new Point3d(profile.PointAtEnd.X, profile.PointAtEnd.Y, baseZ);

                if (start.DistanceTo(end) < Rhino.RhinoMath.ZeroTolerance)
                    return null;
                
                source = GeometrySourceResult.Extrusion;
                return new LineCurve(start, end);
            }

            // Closed extrusion
            var polylineCurve = profile.ToPolyline(
                mainSegmentCount: 0,
                subSegmentCount: 1,
                maxAngleRadians: 0.1,
                maxChordLengthRatio: 0.1,
                maxAspectRatio: 0,
                tolerance: RhinoMath.ZeroTolerance,
                minEdgeLength: RhinoMath.ZeroTolerance,
                maxEdgeLength: double.MaxValue,
                keepStartPoint: true);
            
            if (polylineCurve == null)
                return null;
            
            var polyline = polylineCurve.ToPolyline();

            var segments = polyline.GetSegments();
            if (segments == null || segments.Length == 0)
                return null;
            
            Line longestSegment = segments[0];
            double longestLength = segments[0].Length;

            foreach (var segment in segments)
            {
                if (segment.Length > longestLength)
                {
                    longestLength = segment.Length;
                    longestSegment = segment;
                }
            }

            var segStart = new Point3d(longestSegment.From.X, longestSegment.From.Y, baseZ);
            var segEnd = new Point3d(longestSegment.To.X, longestSegment.To.Y, baseZ);

            var bbox = extrusion.GetBoundingBox(accurate: true);
            var faceCenter = new Point3d(
                (bbox.Min.X + bbox.Max.X) / 2.0,
                (bbox.Min.Y + bbox.Max.Y) / 2.0,
                baseZ
            );

            var edgeDir = segEnd - segStart;
            edgeDir.Unitize();
            var perpDir = Vector3d.CrossProduct(edgeDir, Vector3d.ZAxis);
            perpDir.Unitize();

            var toCenter = faceCenter - segStart;
            double perpDist = toCenter * perpDir;
            var offset = perpDir * perpDist;

            segStart += offset;
            segEnd += offset;

            if (segStart.DistanceTo(segEnd) < RhinoMath.ZeroTolerance)
                return null;
            
            source = GeometrySourceResult.Extrusion;
            return new LineCurve(segStart, segEnd);
        }
    };

    internal enum GeometrySourceResult
    {
        Unknown,
        Curve,
        CurveApproximated,
        Brep,
        Extrusion
    }
}