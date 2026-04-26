using System.ComponentModel;
using System.Linq;
using Intent.Contract.Models;
using Intent.Contract.Validation;

namespace Intent.Core.Validation
{
    public sealed class WallIntentValidator
    {
        public ValidationResult Validate(WallIntent intent)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(intent.SchemaVersion))
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Error,
                    Code = IssueCode.MissingSchemaVersion,
                    Message = "SchemaVersion is required."
                });
            }

            if (string.IsNullOrWhiteSpace(intent.StableId))
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Error,
                    Code = IssueCode.MissingStableId,
                    Message = "StableID is required."
                });
            }

            if (intent.ObjectType != ObjectType.Wall)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Error,
                    Code = IssueCode.InvalidObjectType,
                    Message = "ObjectType must be wall."
                });
            }

            if (intent.LocationCurveStart == null || intent.LocationCurveEnd == null)
            {
                result.Issues.Add(new ValidationIssue
                {
                   Severity = Severity.Error,
                   Code = IssueCode.MissingLocationCurve,
                   Message = "Location curve is missing. Wall cannot be placed without a centerline." 
                });
            }

            if (string.IsNullOrWhiteSpace(intent.TypeName))
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Warning,
                    Code = IssueCode.MissingTypeName,
                    Message = "TypeName is missing. A default wall type will be used."
                });
            }

            if (!intent.NominalWidth.HasValue)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Warning,
                    Code = IssueCode.MissingNominalWidth,
                    Message = "NominalWidth is missing: Revit type selection may be ambiguous."
                });
            }

            if (intent.NominalWidth <= 0)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Error,
                    Code = IssueCode.InvalidNominalWidth,
                    Message = "NominalWidth must be greater than zero."
                });
            }

            if (intent.UnconnectedHeight <= 0 || !intent.UnconnectedHeight.HasValue)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Error,
                    Code = IssueCode.InvalidHeight,
                    Message = "UnconnectedHeight must be greater than zero."
                });
            }

            if (
                intent.UnconnectedHeight.HasValue &&
                intent.UnconnectedHeight > 0 &&
                intent.BaseOffset.HasValue &&
                intent.TopOffset.HasValue
                )
            {
                var effectiveHeight = intent.UnconnectedHeight + intent.TopOffset - intent.BaseOffset;

                if (effectiveHeight <= 0)
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Severity = Severity.Error,
                        Code = IssueCode.InvalidOffsetCombination,
                        Message = $"BaseOffset {intent.BaseOffset}m and TopOffset {intent.TopOffset}m results in negative or zero effective Wall height."
                    });
                }
            }

            if (!intent.LocationLine.HasValue)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Warning,
                    Code = IssueCode.MissingLocationLine,
                    Message = "LocationLine is not set. Revit will default to WallCenterLine."
                });
            }

            if (!intent.IsStructural.HasValue)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Info,
                    Code = IssueCode.MissingStructuralFlag,
                    Message = "IsStructural is not set. Structural coordination may be affected."
                });
            }

            result.IsValid = !result.Issues.Any(x => x.Severity == Severity.Error);

            return result;
        }
    }
}