using System.Collections.Generic;

namespace QueryLogic.ORM.SQL
{
    /// <summary>
    /// Represents a database table
    /// </summary>
    internal class Table
    {
        /// <summary>
        /// Name of the database where the table exists
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Name of the database table
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Name of the code entity the table represents
        /// </summary>
        public string EntityName { get; set; }
        
        /// <summary>
        /// Collection of the table's columns
        /// </summary>
        public List<Column> Columns { get; set; }
    }
}