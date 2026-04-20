using System;
using Intent.Contract.Models;
using Intent.Contract.Validation;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Render.CustomRenderMeshes;

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
            // Step 1 - Select a Curve
            // ----------------------------------------------------------

            var go = new GetObject();
            go.SetCommandPrompt("Select a curve to assign wall intent");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
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
            // Step 2 - Collect wall parameters from the command line
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

            // ----------------------------------------------------------
            // Step 3 - Build the WallIntent DTO
            // ----------------------------------------------------------
            var intent = new WallIntent
            {
                SchemaVersion = "1.0",
                StableId = stableId,
                ObjectType = Contract.Models.ObjectType.Wall,
                TypeName = typeName,
                UnconnectedHeight = height
            };

            // ----------------------------------------------------------
            // Step 4 - Assign, validate, write to UserText
            // ----------------------------------------------------------
            var result = WallIntentService.AssignAndValidate(rhinoObject, intent);

            // ----------------------------------------------------------
            // Step 5 - Apply color override based on validation status
            // ----------------------------------------------------------
            ApplyValidationColor(doc, rhinoObject, result);

            // ----------------------------------------------------------
            // Step 6 - Report to command line
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
                    RhinoApp.WriteLine("    [{0}] {1}: {2}, issue.Severity, issue.Code, issue.Message");
                }
            }

            doc.Views.Redraw();
            return Result.Success;
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