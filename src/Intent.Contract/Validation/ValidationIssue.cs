namespace Intent.Contract.Validation
{
    public sealed class ValidationIssue
    {
        public Severity Severity {get; set; }
        public string Code {get; set; }
        public string Message { get; set; }

        public ValidationIssue()
        {
            Severity = Severity.Info;
            Code = string.Empty;
            Message = string.Empty;
        }
    }
}