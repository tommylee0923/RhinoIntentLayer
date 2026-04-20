using System;
using Intent.Contract.Models;
using Intent.Contract.Validation;
using Intent.Core.Validation;
using Rhino.DocObjects;
using System.Text.Json;

namespace Intent.RhinoLayer
{
    /// <summary>
    /// Handles reading and writing WallIntent data to rhino object
    /// UserText, and running validation. The command classes call into
    /// this. The classes never touch serialization or validation 
    /// directly.
    /// 
    /// UserText key contract:
    ///     Intent.SchemaVersion        → schema version string
    ///     Intent.ObjectType           → "WallIntent"
    ///     Intent.Json                 → serialized WallIntent JSON
    ///     Intent.Validation.Status    → "Valid" | "Invalid"
    ///     Intent.Validation.Json      → serlized ValidationResult JSON
    /// </summary>
    
    internal static class WallIntentService
    {
        // ----------------------------------------------------------
        // Stable UserText keys (NEVER CHANGE THESE AFTER OBJECTS SAVED)
        // ----------------------------------------------------------

        public const string KeySchemaVersion = "Intent.SchemaVersion";
        public const string KeyObjectType = "Intent.ObjectType";
        public const string KeyJson = "Intent.Json";
        public const string KeyValidationStatus = "Intent.Validation.Status";
        public const string KeyValidationJson = "Intent.Validation.Json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // ----------------------------------------------------------
        // Write
        // ----------------------------------------------------------

        /// <summary>
        /// Serializes a WallIntent, runs validation, and writes both to the
        /// object's UserText. Returns the ValidationResult so the caller can apply
        /// visual feedback.
        /// </summary>
        
        public static ValidationResult AssignAndValidate(RhinoObject rhinoObject, WallIntent intent)
        {
            // Serialize intent
            var intentJson = JsonSerializer.Serialize(intent, JsonOptions);

            // Run validation (pure - no Rhino dependency)
            var validator = new WallIntentValidator();
            var result = validator.Validate(intent);

            // Serialize validation result
            var validationJson = JsonSerializer.Serialize(result, JsonOptions);

            // Write all keys to UserText
            var attrs = rhinoObject.Attributes.Duplicate();
            attrs.SetUserString(KeySchemaVersion, intent.SchemaVersion ?? string.Empty);
            attrs.SetUserString(KeyObjectType, "WallIntent");
            attrs.SetUserString(KeyJson, intentJson);
            attrs.SetUserString(KeyValidationStatus, result.IsValid ? "Valid" : "Invalid");
            attrs.SetUserString(KeyValidationJson, validationJson);

            rhinoObject.Document.Objects.ModifyAttributes(rhinoObject, attrs, quiet: true);

            return result;
        }

        // ----------------------------------------------------------
        // Read
        // ----------------------------------------------------------
        /// <summary>
        /// Reads and deserialized a WallIntent from object UserText.
        /// Returns null if the obejct has no Intent data.
        /// </summary>
        public static WallIntent ReadIntent(RhinoObject rhinoObject)
        {
            var json = rhinoObject.Attributes.GetUserString(KeyJson);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<WallIntent>(json, JsonOptions);
        }

        /// <summary>
        /// Reads and deserializes the stored ValidationResult from UserText.
        /// Returns null if no validation has been run on this object.
        /// </summary>
        public static ValidationResult ReadValidationResult(RhinoObject rhinoObject)
        {
            var json = rhinoObject.Attributes.GetUserString(KeyValidationJson);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<ValidationResult>(json, JsonOptions);
        }

        /// <summary>
        /// Returns true if the object has Intent data assigned.
        /// </summary>
        public static bool HasIntent(RhinoObject rhinoObject)
        {
            var objectType = rhinoObject.Attributes.GetUserString(KeyObjectType);
            return objectType == "WallIntent";
        }
    }
}