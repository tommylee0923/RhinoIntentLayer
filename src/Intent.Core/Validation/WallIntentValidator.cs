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
                    Message = "StableId is required."
                });
            }

            if (intent.ObjectType != ObjectType.Wall)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Error,
                    Code = IssueCode.InvalidObjectType,
                    Message = "ObjectType must be Wall."
                });
            }

            if (string.IsNullOrWhiteSpace(intent.TypeName))
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Warning,
                    Code = IssueCode.MissingTypeName,
                    Message = "TypeName is missing."
                });
            }

            if (!intent.UnconnectedHeight.HasValue || intent.UnconnectedHeight.Value <= 0)
            {
                result.Issues.Add(new ValidationIssue
                {
                    Severity = Severity.Error,
                    Code = IssueCode.InvalidHeight,
                    Message = "UnconnectedHeight must be greater than zero."
                });
            }

            result.IsValid = !result.Issues.Any(x => x.Severity == Severity.Error);

            return result;
        }
    }
}