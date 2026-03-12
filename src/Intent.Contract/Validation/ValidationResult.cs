using System.Collections.Generic;

namespace Intent.Contract.Validation
{
    public sealed class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationIssue> Issues {get; set; }

        public ValidationResult()
        {
            IsValid = true;
            Issues = new List<ValidationIssue>();
        }
    }
}