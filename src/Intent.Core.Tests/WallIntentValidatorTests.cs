using System.Formats.Asn1;
using System.IO.Pipelines;
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
        SchemaVersion = "1.0",
        StableId = "wall-001",
        ObjectType = ObjectType.Wall,
        TypeName = "Generic - 200mm",
        NominalWidth = 0.2,
        UnconnectedHeight = 3.0,
        BaseOffset = 0.0,
        TopOffset = 0.0,
        LocationLine = LocationLine.WallCenterline,
        IsStructural = false,
        LocationCurveStart = new double[] {0, 0, 0},
        LocationCurveEnd = new double[] {5, 0, 0}
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
            i.Code == IssueCode.MissingSchemaVersion &&
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
            i.Code == IssueCode.MissingSchemaVersion &&
            i.Severity == Severity.Error);
    }

    [Fact]
    public void Validate_WhitespaceSchemaVersion_ReturnError()
    {
        var intent = ValidIntent();
        intent.SchemaVersion = "   ";

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.MissingSchemaVersion &&
            i.Severity == Severity.Error);
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
            i.Code == IssueCode.MissingStableId &&
            i.Severity == Severity.Error);
    }

    [Fact]
    public void Validate_EmptyStableId_ReturnsError()
    {
        var intent = ValidIntent();
        intent.StableId = "";

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.MissingStableId &&
            i.Severity == Severity.Error);
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
            i.Code == IssueCode.InvalidObjectType &&
            i.Severity == Severity.Error);
    }

    [Fact]
    public void Validate_DefaultConstructedIntent_FailsObjectTypeCheck()
    {

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
            i.Code == IssueCode.MissingTypeName &&
            i.Severity == Severity.Warning);
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
            i.Code == IssueCode.MissingTypeName &&
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
    // WALL_MISSING_NOMINAL_WIDTH / WALL_INVALID_NOMINAL_WIDTH
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_NullNominalWidth_ReturnsWarning()
    {
        var intent = ValidIntent();
        intent.NominalWidth = null;

        var result = Validator().Validate(intent);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.MissingNominalWidth &&
            i.Severity == Severity.Warning);
    }

    [Fact]
    public void Validate_ZeroNominalWidth_ReturnsError()
    {
        var intent = ValidIntent();
        intent.NominalWidth = 0;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.InvalidNominalWidth &&
            i.Severity == Severity.Error);
    }

    [Fact]
    public void Validate_NegativeNominalWidth_ReturnsError()
    {
        var intent = ValidIntent();
        intent.NominalWidth = -0.1;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.InvalidNominalWidth &&
            i.Severity == Severity.Error);
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
            i.Code == IssueCode.InvalidHeight &&
            i.Severity == Severity.Error);
    }

    [Fact]
    public void Validate_ZeroHeight_ReturnError()
    {
        var intent = ValidIntent();
        intent.UnconnectedHeight = 0;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.InvalidHeight);
    }

    [Fact]
    public void Validate_NegativeHeight_ReturnError()
    {
        var intent = ValidIntent();
        intent.UnconnectedHeight = -1.0;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.InvalidHeight);
    }

    // ---------------------------------------------------------------
    // WALL_INVALID_OFFSET_COMBINATION
    // ---------------------------------------------------------------

    [Fact]
    public void Validate_OffsetConsumesHeight_ReturnsError()
    {
        var intent = ValidIntent();
        intent.UnconnectedHeight = 3.0;
        intent.BaseOffset = 4.0;
        intent.TopOffset = 0.0;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.InvalidOffsetCombination &&
            i.Severity == Severity.Error);
    }

    [Fact]
    public void Validate_OffsetExactlyZeroEffectiveHeight_ReturnsError()
    {
        var intent = ValidIntent();
        intent.UnconnectedHeight = 3.0;
        intent.BaseOffset = 3.0;
        intent.TopOffset = 0.0;

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.InvalidOffsetCombination);
    }

    [Fact]
    public void Validate_ValidOffsetCombination_NoIssue()
    {
        var intent = ValidIntent();
        intent.UnconnectedHeight = 3.0;
        intent.BaseOffset = 0.5;
        intent.TopOffset = -0.5;

        var result = Validator().Validate(intent);

        Assert.DoesNotContain(result.Issues, i =>
            i.Code == IssueCode.InvalidOffsetCombination);
    }
    // ------------------------------------------------------------------------
    // WALL_MISSING_LOCATION_LINE (Warning)
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_NullLocationLine_ReturnsWarning()
    {
        var intent = ValidIntent();
        intent.LocationLine = null;

        var result = Validator().Validate(intent);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.MissingLocationLine &&
            i.Severity == Severity.Warning);
    }

    [Fact]
    public void Validate_LocationLineSet_NoIssue()
    {
        var intent = ValidIntent();
        intent.LocationLine = LocationLine.FinishFaceExterior;

        var result = Validator().Validate(intent);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, i =>
            i.Code == IssueCode.MissingLocationLine);
    }
    // ------------------------------------------------------------------------
    // WALL_MISSING_STRUCTURAL_FLAG (Info)
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_NullIsStructural_ReturnsInfo()
    {
        var intent = ValidIntent();
        intent.IsStructural = null;

        var result = Validator().Validate(intent);

        Assert.True(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Code == IssueCode.MissingStructuralFlag &&
            i.Severity == Severity.Info);
    }

    [Fact]
    public void Validate_IsStructuralSet_NoIssue()
    {
        var intent = ValidIntent();
        intent.IsStructural = true;

        var result = Validator().Validate(intent);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, i =>
            i.Code == IssueCode.MissingStructuralFlag);
    }
    // ------------------------------------------------------------------------
    // Severity Contract - Warnings and Info do not invalidate
    // ------------------------------------------------------------------------

    [Fact]
    public void Validate_OnlyWarningAndInfo_IsValidTrue()
    {
        var intent = new WallIntent
        {
            SchemaVersion = "1.0",
            StableId = "wall-001",
            ObjectType = ObjectType.Wall,
            TypeName = null,
            NominalWidth = null,
            UnconnectedHeight = 3.0,
            LocationLine = null,
            IsStructural = null,
            LocationCurveStart = new double[] {0, 0, 0},
            LocationCurveEnd = new double[] {5, 0, 0}
        };

        var result = Validator().Validate(intent);

        Assert.True(result.IsValid);
        Assert.DoesNotContain(result.Issues, i =>
            i.Severity == Severity.Error);

        // When

        // Then
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
            SchemaVersion = "",
            StableId = "",
            ObjectType = ObjectType.Unknown,
            TypeName = "",
            NominalWidth = -1.0,
            UnconnectedHeight = 0,
            BaseOffset = 10,
            TopOffset = -10,
            LocationLine = null,
            IsStructural = null
        };

        var result = Validator().Validate(intent);

        Assert.False(result.IsValid);
        Assert.Equal(9, result.Issues.Count);
    }
}