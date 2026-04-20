using System.Runtime.InteropServices.Marshalling;
using Intent.Contract.Models;
using Intent.Contract.Validation;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.Render.CustomRenderMeshes;

namespace Intent.RhinoLayer
{
    /// <summary>
    /// Rhino command: InsepctWallIntent
    /// 
    /// Selects a curve, reads the stored WallIntent and ValidationResult
    /// from UserText, and prints a human-readable summary to the Rhino
    /// command line.
    /// </summary>
    public class InsepctWallIntentCommand : Command
    {
        public InsepctWallIntentCommand()
        {
            Instance = this;
        }

        public static InsepctWallIntentCommand Instance { get; private set; }

        public override string EnglishName => "InspectWallIntent";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // ----------------------------------------------------------
            // Step 1 - Select a Curve
            // ----------------------------------------------------------

            var go = new GetObject();
            go.SetCommandPrompt("Select a curve to inspect");
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
            // Step 2 - Guard: check if intent has been assigned
            // ----------------------------------------------------------

            if (!WallIntentService.HasIntent(rhinoObject))
            {
                RhinoApp.WriteLine("This object has no WallIntent assigned.");
                RhinoApp.WriteLine("Run AssignWallIntent first.");
                return Result.Nothing;
            }

            // ----------------------------------------------------------
            // Step 3 - Read and deserialize intent
            // ----------------------------------------------------------

            var intent = WallIntentService.ReadIntent(rhinoObject);
            if (intent == null)
            {
                RhinoApp.WriteLine("Error: Intent data could not be read.");
                return Result.Failure;
            }

            // ----------------------------------------------------------
            // Step 4 - Read and deserialize validation result
            // ----------------------------------------------------------

            var validation = WallIntentService.ReadValidationResult(rhinoObject);

            // ----------------------------------------------------------
            // Step 5 - Print summary
            // ----------------------------------------------------------

            PrintIntent(intent);
            PrintValidation(validation);

            return Result.Success;
        }

        // ----------------------------------------------------------
        // Printing helpers
        // ----------------------------------------------------------

        private static void PrintIntent(WallIntent intent)
        {
            RhinoApp.WriteLine("-------------------------------------");
            RhinoApp.WriteLine("WallIntent");
            RhinoApp.WriteLine("-------------------------------------");
            RhinoApp.WriteLine($"   StableId:           {intent.StableId ?? "none"}");
            RhinoApp.WriteLine($"   SchemaVersion:      {intent.SchemaVersion ?? "none"}");
            RhinoApp.WriteLine($"   ObjectType:         {intent.ObjectType}");
            RhinoApp.WriteLine($"   TypeName:           {(string.IsNullOrWhiteSpace(intent.TypeName) ? "none" : intent.TypeName)}");
            RhinoApp.WriteLine($"   UnconnectedHeight:  {(intent.UnconnectedHeight.HasValue ? intent.UnconnectedHeight.Value.ToString("F2") + "m" : "none")}");
        }

        private static void PrintValidation(ValidationResult validation)
        {
            RhinoApp.WriteLine("-------------------------------------");
            RhinoApp.WriteLine("Validation");
            RhinoApp.WriteLine("-------------------------------------");

            if (validation == null)
            {
                RhinoApp.WriteLine("    No validataion result stored.");
                RhinoApp.WriteLine("-------------------------------------");
                return;
            }

            RhinoApp.WriteLine("   Status: {0}", validation.IsValid ? "Valid" : "Invalid");

            if (validation.Issues.Count == 0)
            {
                RhinoApp.WriteLine("    Issues: None");
            }
            else
            {
                RhinoApp.WriteLine("    Issues:");
                foreach (var issue in validation.Issues)
                {
                    RhinoApp.WriteLine($"   [{issue.Severity}] {issue.Code}: {issue.Message}");
                }
            }

            RhinoApp.WriteLine("-------------------------------------");
        }
    }
}