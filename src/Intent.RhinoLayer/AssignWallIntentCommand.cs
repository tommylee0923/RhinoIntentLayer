using System;
using Intent.Contract.Models;
using Intent.Contract.Validation;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace Intent.RhinoLayer
{
    /// <summary>
    /// Rhino command: AssignWallIntent
    /// 
    /// Prompts the user to select a curve, collects wall parameters, assigns
    /// a WallIntent to the object, runs validation, and applies a color override
    /// refllect the validation status.
    /// 
    /// This class is intentionally thin - all serialization, validation, and 
    /// UserText logic lives in WallIntentService.
    /// </summary>
    public class AssignWallIntentCommand : Command
    {
        public AssignWallIntentCommand()
        {
            Instance = this;
        }

        public static AssignWallIntentCommand Instance
        {
            get;
            private set;
        }

        public override string EnglishName => "AssignWallIntent";
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // ----------------------------------------------------------
            // Step 1 - Select a Brep or Curve
            // ----------------------------------------------------------

            var go = new GetObject();
            go.SetCommandPrompt("Select a wall solid or curve to assign wall intent");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve | Rhino.DocObjects.ObjectType.Brep;
            go.SubObjectSelect = false;
            go.Get();

            if (go.CommandResult() != Result.Success)
            {
                return go.CommandResult();
            }

            var rhinoObject = go.Object(0).Object();
            if (rhinoObject == null)
            {
                RhinoApp.WriteLine("No object selected.");
                return Result.Failure;
            }

            // ----------------------------------------------------------
            // Step 2 - Extract wall centerline from geometry
            // ----------------------------------------------------------
            var locationCurve = WallGeometryExtractor.TryExtract(rhinoObject, out var sourceResult);

            if (locationCurve == null)
            {
                RhinoApp.WriteLine("Could not derive a wall centerline from the selected geometry.");
                RhinoApp.WriteLine("Select a box-like solid or a linear curve.");
                return Result.Failure;
            }

            var geometrySource = MapGeometrySource(sourceResult);

            if (sourceResult == GeometrySourceResult.CurveApproximated)
                RhinoApp.WriteLine("Note: Non-linear curve approximated as straight line.");

            RhinoApp.WriteLine($"Location line derived from {geometrySource}. " +
                               $"Length: {locationCurve.GetLength():F2}m");

            // ----------------------------------------------------------
            // Step 3 - Collect wall parameters from the command line
            // ----------------------------------------------------------
            var stableId = Guid.NewGuid().ToString();

            // Height
            var getHeight = new GetNumber();
            getHeight.SetCommandPrompt("Wall unconnected height(m)");
            getHeight.SetDefaultNumber(3.0);
            getHeight.SetLowerLimit(0.01, strictlyGreaterThan: true);
            getHeight.Get();
            if (getHeight.CommandResult() != Result.Success)
            {
                return getHeight.CommandResult();
            }
            var height = getHeight.Number();

            // Type name
            var getType = new GetString();
            getType.SetCommandPrompt("Wall type name (press Enter to skip):");
            getType.SetDefaultString("Generic - 200mm");
            getType.AcceptNothing(true);
            getType.Get();

            if (getType.CommandResult() != Result.Success)
            {
                return getType.CommandResult();
            }
            var typeName = getType.StringResult();

            // Nominal Width
            var getWidth = new GetNumber();
            getWidth.SetCommandPrompt("Nominal wall width (m, press enter to skip)");
            getWidth.SetDefaultNumber(0.2);
            getWidth.AcceptNothing(true);
            getWidth.Get();

            if (getWidth.CommandResult() != Result.Success)
            {
                return getWidth.CommandResult();
            }

            double? nominalWidth = getWidth.CommandResult() == Result.Nothing
                ? (double?)null
                : getWidth.Number();

            // Base Offset
            var getBaseOffset = new GetNumber();
            getBaseOffset.SetCommandPrompt("Base offset (m, press Enter for 0)");
            getBaseOffset.SetDefaultNumber(0.0);
            getBaseOffset.AcceptNothing(true);
            getBaseOffset.Get();

            if (getBaseOffset.CommandResult() != Result.Success)
            {
                return getBaseOffset.CommandResult();
            }

            double baseOffset = getBaseOffset.CommandResult() == Result.Nothing
                ? 0.0
                : getBaseOffset.Number();

            // Top Offset
            var getTopOffset = new GetNumber();
            getTopOffset.SetCommandPrompt("Top offset (m, press Enter for 0)");
            getTopOffset.SetDefaultNumber(0.0);
            getTopOffset.AcceptNothing(true);
            getTopOffset.Get();

            if (getTopOffset.CommandResult() != Result.Success)
            {
                return getTopOffset.CommandResult();
            }

            double topOffset = getTopOffset.CommandResult() == Result.Nothing
                ? 0.0
                : getTopOffset.Number();

            // Location Line
            var getLocationLine = new GetString();
            getLocationLine.SetCommandPrompt(
                "Location line (0=WallCenterline, 1=CoreCenterline, 2=FinishFaceExterior," +
                "3=FinishFaceInterior, 4=CoreFaceExterior, 5=CoreFaceInterior, Enter to skip)"
                );
            getLocationLine.SetDefaultString("0");
            getLocationLine.AcceptNothing(true);
            getLocationLine.Get();

            if (getLocationLine.CommandResult() != Result.Success)
            {
                return getLocationLine.CommandResult();
            }

            LocationLine? locationline = null;
            if (getLocationLine.CommandResult() != Result.Nothing &&
                int.TryParse(getLocationLine.StringResult(), out int locationLineValue) &&
                System.Enum.IsDefined(typeof(LocationLine), locationLineValue))
            {
                locationline = (LocationLine)locationLineValue;
            }

            // IsStructural
            var getIsStructural = new GetString();
            getIsStructural.SetCommandPrompt("Is it structural? (y/n, press Enter to skip)");
            getIsStructural.AcceptNothing(true);
            getIsStructural.Get();

            if (getIsStructural.CommandResult() != Result.Success)
            {
                return getIsStructural.CommandResult();
            }

            bool? isStructural = null;
            if (getIsStructural.CommandResult() != Result.Nothing)
            {
                var input = getIsStructural.StringResult().Trim().ToLower();
                if (input == "y" || input == "yes") isStructural = true;
                else if (input == "n" || input == "no") isStructural = false;
            }

            // ----------------------------------------------------------
            // Step 4 - Build the WallIntent DTO
            // ----------------------------------------------------------
            var intent = new WallIntent
            {
                SchemaVersion = "1.0",
                StableId = stableId,
                ObjectType = Contract.Models.ObjectType.Wall,
                TypeName = typeName,
                NominalWidth = nominalWidth,
                UnconnectedHeight = height,
                BaseOffset = baseOffset,
                TopOffset = topOffset,
                LocationLine = locationline,
                IsStructural = isStructural
            };

            // ----------------------------------------------------------
            // Step 5 - Assign, validate, write to UserText
            // ----------------------------------------------------------
            var result = WallIntentService.AssignAndValidate(
                rhinoObject, intent, locationCurve, geometrySource);

            // ----------------------------------------------------------
            // Step 6 - Apply color override based on validation status
            // ----------------------------------------------------------
            ApplyValidationColor(doc, rhinoObject, result);

            // ----------------------------------------------------------
            // Step 7 - Report to command line
            // ----------------------------------------------------------
            if (result.IsValid)
            {
                RhinoApp.WriteLine("WallIntent assigned successfully. Status: Valid");
            }
            else
            {
                RhinoApp.WriteLine("WallIntent assigned with issues. Status: Invalid.");
                foreach (var issue in result.Issues)
                {
                    RhinoApp.WriteLine($"    [{issue.Severity}] {issue.Code}: {issue.Message}");
                }
            }

            doc.Views.Redraw();
            return Result.Success;
        }

        // ----------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------
        private static GeometrySource MapGeometrySource(GeometrySourceResult result)
        {
            switch (result)
            {
                case GeometrySourceResult.Curve:
                case GeometrySourceResult.CurveApproximated:
                    return GeometrySource.Curve;
                case GeometrySourceResult.Brep:
                    return GeometrySource.Brep;
                case GeometrySourceResult.Extrusion:
                    return GeometrySource.Extrusion;
                default:
                    return GeometrySource.Unknown;
            }
        }
        
        private static void ApplyValidationColor(RhinoDoc doc, RhinoObject rhinoObject, ValidationResult result)
        {
            var attrs = rhinoObject.Attributes.Duplicate();
            attrs.ColorSource = ObjectColorSource.ColorFromObject;

            if (!result.IsValid)
            {
                // Error - red
                attrs.ObjectColor = System.Drawing.Color.FromArgb(220, 53, 69);
            }
            else if (result.Issues.Count > 0)
            {
                // Valid but has warnings - orange
                attrs.ObjectColor = System.Drawing.Color.FromArgb(255, 140, 0);
            }
            else
            {
                // Fully valid - green
                attrs.ObjectColor = System.Drawing.Color.FromArgb(40, 167, 69);
            }

            doc.Objects.ModifyAttributes(rhinoObject, attrs, quiet: true);
        }

    }
}