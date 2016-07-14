namespace QueryLogic.ORM
{
    /// <summary>
    /// Container for logical operators for searches.
    /// </summary>
    public struct SearchOperators
    {
        /// <summary>
        /// Equals to
        /// </summary>
        public const string Equal = "=";
        
        /// <summary>
        /// Less than
        /// </summary>
        public const string LessThan = "<";
        
        /// <summary>
        /// Less than or equal to
        /// </summary>
        public const string LessThanOrEqualTo = "<=";
        
        /// <summary>
        /// Greater than
        /// </summary>
        public const string GreaterThan = ">";
        
        /// <summary>
        /// Greater than or equal to
        /// </summary>
        public const string GreaterThanOrEqualTo = ">=";
        
        /// <summary>
        /// Like (for "contains" searches)
        /// </summary>
        public const string Like = "like";
        
        /// <summary>
        /// Or
        /// </summary>
        public const string Or = "or";
        
        /// <summary>
        /// Not equal to
        /// </summary>
        public const string NotEqual = "<>";
        
        /// <summary>
        /// Is null
        /// </summary>
        public const string IsNull = "is null";
        
        /// <summary>
        /// Is not null
        /// </summary>
        public const string IsNotNull = "is not null";
        
        /// <summary>
        /// Equal to an element in a sequence
        /// </summary>
        public const string In = "in";

        /// <summary>
        /// Not equal to an element in a sequence
        /// </summary>
        public const string NotIn = "not in";
    }
}