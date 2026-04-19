using System.Formats.Asn1;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ObjectiveC;
using System.Xml.Schema;
using Intent.Contract.Models;
using Intent.Contract.Validation;
using Intent.Core.Validation;
using Xunit;

namespace Intent.Core.Tests.Validation;

/// <summary>
/// Unit tests for WallIntentValidator
/// Each test follows the Arrange / Act / Assert pattern and convers exactly
/// one rule. Tests are independent: each builds its own WallIntent from a
/// known-good baseline so a change to one ruile cannot silently break another
/// test.
/// </summary>

public sealed class WallIntentValidatorTests
{
    // ------------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------------

    /// <summary>
    /// Returns a WallIntent that passes every validation rule.
    /// Individual tests mutate one field to probe a specific rule.
    /// </summary>
    
    private static WallIntent ValidIntent() => new WallIntent
    {
        SchemaVersion   = "1.0",
        StableId        = "wall-001",
        ObjectType      = ObjectType.Wall,
        TypeName        = "Generic - 200mm",
        UnconnectedHeight = 3.0
    };

    private static WallIntentValidator Validator() => new WallIntentValidator();

    // ------------------------------------------------------------------------
    // Happy path
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_ValidIntent_ReturnIsValidTrue()
    {
        var result = Validator().Validate(ValidIntent());

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    // ------------------------------------------------------------------------
    // COMMON_MISSING_SCHEMA_VERSION
    // ------------------------------------------------------------------------
    
    [Fact]
    public void Validate_NullSchemaVersion_ReturnsError()
    {
        var intent = ValidIntent();
        intent.SchemaVersion = null;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code      == IssueCode.MissingSchemaVersion &&
            i.Severity == Severity.Error);
    }

    [Fact]
    public void Validate_EmptySchemaVersion_ReturnsError()
    {
        var intent = ValidIntent();
        intent.SchemaVersion = "";

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code          == IssueCode.MissingSchemaVersion);
    }

    [Fact]
    public void Validate_WhitespaceSchemaVersion_ReturnError()
    {
        var intent = ValidIntent();
        intent.SchemaVersion = "   ";

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.MissingSchemaVersion);
    }

    // ------------------------------------------------------------------------
    // WALL_MISSING_STABLE_ID
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_NullStableId_ReturnsError()
    {
        var intent = ValidIntent();
        intent.StableId = null;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code      == IssueCode.MissingStableId &&
            i.Severity  == Severity.Error);
    }

    [Fact]
    public void Validate_EmptyStableId_ReturnsError()
    {
        var intent = ValidIntent();
        intent.StableId = "";

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.MissingStableId);
    }

    // ------------------------------------------------------------------------
    // WALL_INVALID_OBJECT_TYPE
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_ObjectTypeUnknown_ReturnsError()
    {
        var intent = ValidIntent();
        intent.ObjectType = ObjectType.Unknown;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code      == IssueCode.InvalidObjectType &&
            i.Severity  == Severity.Error);
    }

    [Fact]
    public void Validate_DefaultConstructedIntent_FailsObjectTypeCheck()
    {
        // WallIntent() defaults ObjectType to Unknown - ensure the validator
        // catches this so callers cannot forget to see it.

        var intent = new WallIntent();

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.InvalidObjectType);
    }

    [Fact]
    public void Validate_WhitespaceTypeName_ReturnsWarning()
    {
        var intent = ValidIntent();
        intent.TypeName = "   ";

        var result = Validator().Validate(intent);

        Assert.Contains(result.Issues, i => 
            i.Code      == IssueCode.MissingTypeName &&
            i.Severity  == Severity.Warning);
    }

    // ------------------------------------------------------------------------
    // WALL_MISSING_TYPE_NAME (Warning - does not invalidate result)
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_MissingTypeName_ReturnsWarning()
    {
        var intent = ValidIntent();
        intent.TypeName = null;

        var result = Validator().Validate(intent);

        Assert.Contains(result.Issues, i =>
            i.Code  == IssueCode.MissingTypeName &&
            i.Severity == Severity.Warning);
    }

    [Fact]
    public void Validate_MissingTypeName_DoesNotInvalidateResult()
    {
        // TypeName is a warning - IsValid must stay true when it is the only
        // issue present.
        var intent = ValidIntent();
        intent.TypeName = "";

        var result = Validator().Validate(intent);

        Assert.True(result.IsValid);
    }

    // ------------------------------------------------------------------------
    // WALL_INVALID_HEIGHT
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_NullHeight_ReturnsError()
    {
        var intent = ValidIntent();
        intent.UnconnectedHeight = null;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code      == IssueCode.InvalidHeight &&
            i.Severity  == Severity.Error);
    }

    [Fact]
    public void Validate_ZeroHeight_ReturnError()
    {
        var intent = ValidIntent();
        intent.UnconnectedHeight = 0;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code      == IssueCode.InvalidHeight);
    }

    [Fact]
    public void Validate_NegativeHeight_ReturnError()
    {
        var intent = ValidIntent();
        intent.UnconnectedHeight = -1.0;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code      == IssueCode.InvalidHeight);
    }

    // ------------------------------------------------------------------------
    // Multi-failure accumulation
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_MultipleViolations_ReturnsAllIssues()
    {
        // Confirms the validator accumulates all issues (5 issues currently)
        // rather than short-circuiting on the first failure.

        var intent = new WallIntent
        {
            SchemaVersion       = "",
            StableId            = "",
            ObjectType          = ObjectType.Unknown,
            TypeName            = "",
            UnconnectedHeight   = 0
        };

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Equal(5, result.Issues.Count);
    }
}