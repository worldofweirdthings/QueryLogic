namespace QueryLogic.ORM.SQL
{
    /// <summary>
    /// Represents a database column
    /// </summary>
    internal class Column
    {
        /// <summary>
        /// Name of the database column
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Name of the object property the column represents
        /// </summary>
        public string PropertyName { get; set; }
        
        /// <summary>
        /// Column data type
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// A cell value for the column
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// Flag denoting if the column is a primary key or a
        /// part of a primary key on its parent table
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        
        /// <summary>
        /// If the column is a primary key, denotes if it's
        /// generated autimatically or manually
        /// </summary>
        public string SequenceType { get; set; }

        /// <summary>
        /// Flag denoting if the column is a foreign key
        /// </summary>
        public bool IsForeignKey { get; set; }
        
        /// <summary>
        /// If the column is a foreign key, denotes the
        /// table which this column is referencing
        /// </summary>
        public string ReferenceTable { get; set; }

        /// <summary>
        /// If the column is a foreign key, denotes the
        /// column in the table which it is referencing
        /// </summary>
        public string ReferenceColumn { get; set; }

        /// <summary>
        /// Indicates if cell data is to be populated only on insert (dafault is false)
        /// </summary>
        public bool CreateOnly { get; set; }
    }
}