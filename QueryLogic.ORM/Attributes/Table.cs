using System;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Represents a database table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class Table : Attribute
    {
        /// <summary>
        /// Creates a new instance of the attribute
        /// </summary>
        public Table() { }

        /// <summary>
        /// Creates a new instance of the attribute
        /// </summary>
        /// <param name="name">Name of the database table</param>
        public Table(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates a new instance of the attribute
        /// </summary>
        /// <param name="name">Name of the database table</param>
        /// <param name="database">Name of the database</param>
        public Table(string name, string database)
        {
            Name = name;
            Database = database;
        }
        
        /// <summary>
        /// Name of the database table
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name of the database connection
        /// </summary>
        public string Database { get; set; }
    }
}