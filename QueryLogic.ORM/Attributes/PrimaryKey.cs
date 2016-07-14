using System;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Denotes a primary key or a part of a compound primary key of a table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]    
    public class PrimaryKey : Attribute
    {
        /// <summary>
        /// Detones whether the key is set manually or automatically
        /// </summary>
        public string Sequence { get; set; }
    }
}