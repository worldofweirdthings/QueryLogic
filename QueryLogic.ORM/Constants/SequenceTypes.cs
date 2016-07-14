namespace QueryLogic.ORM
{
    /// <summary>
    /// Represents how a primary key is set
    /// </summary>
    public struct SequenceTypes
    {
        /// <summary>
        /// Key generation technique not specified
        /// </summary>
        public const string None = "None";

        /// <summary>
        /// Key is an identity or a sequence and generated automatically
        /// </summary>
        public const string Automatic = "Automatic";

        /// <summary>
        /// Key is set manually
        /// </summary>
        public const string Manual = "Manual";
    }
}