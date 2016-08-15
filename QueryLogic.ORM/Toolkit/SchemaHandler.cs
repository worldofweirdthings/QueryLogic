using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Utility for parsing entity attributes to create a representation of a database table
    /// </summary>
    internal static class SchemaHandler
    {
        private static Dictionary<string, SQL.Table> _cache => new Dictionary<string, SQL.Table>();
        
        /// <summary>
        /// Generates a representation of a database table from an entity's attributes
        /// </summary>
        /// <param name="entity">Instance of an entity</param>
        /// <returns>Representation of a database table</returns>
        internal static SQL.Table GetTableSchema(object entity)
        {
            var entityType = entity.GetType();
            var tableInfo = entityType.GetCustomAttributes(typeof(Table), true).First() as Table;
            var table = new SQL.Table
            {
                Name = tableInfo != null ? tableInfo.Name : entityType.Name,
                EntityName = entityType.Name,
                Columns = new List<SQL.Column>()
            };

            if (_cache.ContainsKey(table.Name)) return _cache[table.Name];

            foreach (var property in entityType.GetProperties())
            {
                var type = property.PropertyType;

                var columnAttr = property.GetCustomAttribute<Column>();
                var primaryKey = property.GetCustomAttribute<PrimaryKey>();
                var foreignKey = property.GetCustomAttribute<ForeignKey>();

                string typeName;
                object propertyValue;

                if (foreignKey != null)
                {
                    if (property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null) throw new QueryException("Many to many relationships are not supported for the Foreign Key attribute.");

                    var refEntityValue = property.GetValue(entity);
                    var refEntityTable = GetTableSchema(refEntityValue ?? Activator.CreateInstance(type));
                    var refEntityFKColumn = refEntityTable.Columns.First(c => c.Name == foreignKey.ReferenceColumn);
                        
                    typeName = refEntityFKColumn.Type;
                    propertyValue = refEntityFKColumn.Value;
                }
                else
                {
                    typeName = type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>) ? type.GetGenericArguments()[0].FullName : type.FullName;
                    propertyValue = property.GetValue(entity, null);
                }

                var column = new SQL.Column
                {
                    Name = columnAttr != null ? columnAttr.Name : property.Name,
                    PropertyName = property.Name,
                    Value = propertyValue,
                    Type = typeName,
                    IsPrimaryKey = primaryKey != null,
                    IsForeignKey = foreignKey != null,                    
                    SequenceType = primaryKey != null ? primaryKey.Sequence : SequenceTypes.Automatic,
                    ReferenceTable = (!string.IsNullOrEmpty(foreignKey?.ReferenceTable)) ? foreignKey.ReferenceTable : string.Empty,
                    ReferenceColumn = (!string.IsNullOrEmpty(foreignKey?.ReferenceColumn)) ? foreignKey.ReferenceColumn : string.Empty,
                    CreateOnly = columnAttr?.CreateOnly ?? false
                };
                
                table.Columns.Add(column);
            }

            _cache.Add(table.Name, table);

            return table;
        }

        /// <summary>
        /// Generates representations of database tables from entity attributes
        /// </summary>
        /// <param name="entities">Instances of an entity</param>
        /// <returns>Representations of database tables</returns>
        internal static List<SQL.Table> GetTableSchemas(IEnumerable<object> entities)
        {
            return entities.Select(GetTableSchema).ToList();
        }
    }
}