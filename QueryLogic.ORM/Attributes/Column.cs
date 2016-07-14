using System;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Represents a database table column
    /// </summary>    
    [AttributeUsage(AttributeTargets.Property)]
    public class Column : Attribute
    {
        /// <summary>
        /// Creates a new instance of the attribute
        /// </summary>
        public Column() { }

        /// <summary>
        /// Creates a new instance of the attribute
        /// </summary>
        /// <param name="name">Name of the database table column</param>
        /// <param name="createOnly">Indicates if cell data is to be populated only on insert (dafault is false)</param>
        public Column(string name, bool createOnly = false)
        {
            Name = name;
            CreateOnly = createOnly;
        }
        
        /// <summary>
        /// Name of the database table column
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates if cell data is to be populated only on insert (dafault is false)
        /// </summary>
        public bool CreateOnly { get; set; }
    }
}