namespace Intent.Contract.Validation
{
    public static class IssueCode
    {
    // -------------------------------------------------------------------
    // WALL
    // -------------------------------------------------------------------
        // ---------------------------------------------------------------
        // Common
        // ---------------------------------------------------------------
        public const string MissingSchemaVersion    = "COMMON_MISSING_SCHEMA_VERSION";

        // ---------------------------------------------------------------
        // identity
        // ---------------------------------------------------------------
        public const string MissingStableId         = "WALL_MISSING_STABLE_ID";
        public const string InvalidObjectType       = "WALL_INVALID_OBJECT_TYPE";

        // ---------------------------------------------------------------
        // type-level hints
        // ---------------------------------------------------------------
        public const string MissingTypeName         = "WALL_MISSING_TYPE_NAME";
        public const string MissingNominalWidth     = "WALL_MISSING_NOMINAL_WIDTH";
        public const string InvalidNominalWidth     = "WALL_INVALID_NOMINAL_WIDTH";

        // ---------------------------------------------------------------
        // instance properties
        // ---------------------------------------------------------------
        public const string InvalidHeight           = "WALL_INVALID_HEIGHT";
        public const string InvalidBaseOffset       = "WALL_INVALID_BASE_OFFSET";
        public const string InvalidTopOffset        = "WALL_INVALID_TOP_OFFSET";
        public const string MissingLocationLine     = "WALL_MISSING_LOCATION_LINE";
        public const string MissingStructuralFlag   = "WALL_MISSING_STRUCTURAL_FLAG";
    }
}