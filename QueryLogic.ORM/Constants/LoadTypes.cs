namespace QueryLogic.ORM
{
    /// <summary>
    /// Controls how data is collected by the ORM
    /// </summary>
    public struct LoadTypes
    {
        /// <summary>
        /// Data is collected only for the requested entity/entities
        /// </summary>
        public const string Lazy = "Lazy";
        
        /// <summary>
        /// Data is collected for the requested entity/entities and related objects
        /// </summary>
        public const string Eager = "Eager";
    }
}