using System;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Represents a foreign key relationship with another database table
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKey : Attribute
    {
        /// <summary>
        /// Creates a new instance of the attribute
        /// </summary>
        public ForeignKey() { }

        /// <summary>
        /// Creates a new instance of the attribute
        /// </summary>
        /// <param name="referenceTable">Table with which the relationship exists</param>
        /// <param name="referenceColumn">Column name of the foreign key</param>
        public ForeignKey(string referenceTable, string referenceColumn)
        {
            ReferenceTable = referenceTable;
            ReferenceColumn = referenceColumn;
        }

        /// <summary>
        /// Table with which the relationship exists
        /// </summary>
        public string ReferenceTable { get; set; }

        /// <summary>
        /// Column name of the foreign key
        /// </summary>
        public string ReferenceColumn { get; set; }
    }
}