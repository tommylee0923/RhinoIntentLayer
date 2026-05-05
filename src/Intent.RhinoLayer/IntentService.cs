using System;
using Intent.Contract.Models;
using Intent.Contract.Validation;
using Intent.Contract.Serialization;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Intent.RhinoLayer
{
    /// <summary>
    /// Generic service for reading and writing any intent type to
    /// Rhino object UserText.
    /// 
    /// Type-specific serialization and validation are passed in as
    /// delegates.
    /// 
    /// UserText key contract:
    ///     Intent.SchemaVersion        → schema version string
    ///     Intent.ObjectType           → e.g. "WallIntent"
    ///     Intent.Json                 → serialized intent JSON
    ///     Intent.Validation.Status    → "Valid" | "Invalid"
    ///     Intent.Validation.Json      → serlized ValidationResult JSON
    /// </summary>
    internal static class IntentService
    {
        // ----------------------------------------------------------
        // Stable UserText keys (NEVER CHANGE THESE AFTER OBJECTS SAVED)
        // ----------------------------------------------------------

        public const string KeySchemaVersion = "Intent.SchemaVersion";
        public const string KeyObjectType = "Intent.ObjectType";
        public const string KeyJson = "Intent.Json";
        public const string KeyValidationStatus = "Intent.Validation.Status";
        public const string KeyValidationJson = "Intent.Validation.Json";

        // ----------------------------------------------------------
        // Write
        // ----------------------------------------------------------

        public static ValidationResult AssignAndValidate<TIntent>(
            RhinoObject rhinoObject,
            TIntent intent,
            string objectTypeLabel,
            Func<TIntent, string> serialize,
            Func<TIntent, ValidationResult> validate,
            LineCurve locationCurve,
            GeometrySource geometrySource)
            where TIntent : IIntent
        {
            if (locationCurve != null && intent is ICurveIntent curveIntent)
            {
                curveIntent.GeometrySource = geometrySource;
                curveIntent.LocationCurveStart = PointToArray(locationCurve.PointAtStart);
                curveIntent.LocationCurveEnd = PointToArray(locationCurve.PointAtEnd);
            }
            else
            {
                intent.GeometrySource = geometrySource;
            }

            var intentJson = serialize(intent);
            var result = validate(intent);
            var validationJson = IntentJson.SerializeValidationResult(result);

            var attrs = rhinoObject.Attributes.Duplicate();
            attrs.SetUserString(KeySchemaVersion, intent.SchemaVersion ?? string.Empty);
            attrs.SetUserString(KeyObjectType, objectTypeLabel);
            attrs.SetUserString(KeyJson, intentJson);
            attrs.SetUserString(KeyValidationStatus, result.IsValid ? "Valid" : "Invalid");
            attrs.SetUserString(KeyValidationJson, validationJson);

            rhinoObject.Document.Objects.ModifyAttributes(rhinoObject, attrs, quiet:true);

            return result;
        }

        // ----------------------------------------------------------
        // Read
        // ----------------------------------------------------------

        public static TIntent ReadIntent<TIntent>(
            RhinoObject rhinoObject,
            Func<string, TIntent> deserialize)
        {
            var json = rhinoObject.Attributes.GetUserString(KeyJson);

            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return deserialize(json);
        }

        public static ValidationResult ReadValidationResult(RhinoObject rhinoObject)
        {
            var json = rhinoObject.Attributes.GetUserString(KeyValidationJson);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return IntentJson.DeserializeValidationResult(json);
        }

        public static bool HasIntent(RhinoObject rhinoObject, string objectTypeLabel)
        {
            var stored = rhinoObject.Attributes.GetUserString(KeyObjectType);
            return stored == objectTypeLabel;
        }

        // ----------------------------------------------------------
        // Helper
        // ----------------------------------------------------------
        private static double[] PointToArray(Point3d point)
        {
            return new double[] { point.X, point.Y, point.Z };
        }
    }
}