using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

namespace QueryLogic.ORM
{
    /// <summary>
    /// Handler for ORM's interaction with the database
    /// </summary>
    public class SqlHandler : ISqlHandler
    {
        private readonly IQueryBuilder _queryBuilder;

        /// <summary>
        /// Creates instance of the ORM handler
        /// </summary>
        /// <param name="queryBuilder">Injected instance of the QueryBuilder utility</param>
        public SqlHandler(IQueryBuilder queryBuilder)
        {
            _queryBuilder = queryBuilder;
        }

        /// <summary>
        /// Find objects in a database meeting specified criteria
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <param name="search">Search criteria</param>
        /// <returns>Collection of requested objects</returns>
        public SearchResults<T> Find<T>(Search search)
        {
            if ((search.Rows.HasValue && !search.Offset.HasValue) || (search.Offset.HasValue && !search.Rows.HasValue))
                throw new QueryException("Paginated requests must specify both row offset and number of rows requested.");

            if (!search.SearchTerms.Any())
                throw new QueryException("Search requests must specify at least one search term.");

            var entity = Activator.CreateInstance<T>();
            var table = SchemaHandler.GetTableSchema(entity);
            var properties = entity.GetType().GetProperties();

            var joins = new StringBuilder();
            var orderByClause = string.Empty;
            var clauseMap = new Dictionary<string, StringBuilder>
            {
                { "and", new StringBuilder() },
                { "or", new StringBuilder() }
            };

            var countSelect = $"select count([{setPKSelect(getPrimaryKeys(table))}]) as total from [{table.Name}] ";
            var resultSelect = setSelectClause(table);

            foreach (var searchTerm in search.SearchTerms)
            {
                var column = table.Columns.FirstOrDefault(x => x.PropertyName == searchTerm.SearchObject);

                if (searchTerm.SearchObject.Contains("."))
                {
                    var parsedTerm = searchTerm.SearchObject.Split('.');

                    for (var x = 0; x < parsedTerm.Length; x++)
                    {
                        if (x == 0)
                        {
                            joins.Append(setJoin(parsedTerm[x], table));

                            continue;
                        }

                        var refEntityType = properties.First(p => p.Name == parsedTerm[x - 1]).PropertyType;
                        var refEntity = Activator.CreateInstance(refEntityType);
                        var refEntityTable = SchemaHandler.GetTableSchema(refEntity);

                        if (x < parsedTerm.Length - 1)
                        {
                            joins.Append(setJoin(parsedTerm[x], refEntityTable));
                        }
                        else
                        {
                            column = refEntityTable.Columns.FirstOrDefault(c => c.PropertyName == parsedTerm[x]);
                        }
                    }
                }

                if (column == null) throw new QueryException("Column metadata for the specified search property is not set.");

                var term = $"[{column.Name}] {searchTerm.SearchOperator} {string.Format(SqlFormat.Format(column.Type), searchTerm.ObjectValue)}";

                clauseMap[searchTerm.Condition].Append($"{(clauseMap[searchTerm.Condition].Length == 0 ? " " : $" {searchTerm.Condition} ")}{term}");

                if (string.IsNullOrEmpty(search.OrderBy)) continue;

                var orderBy = table.Columns.FirstOrDefault(c => c.PropertyName == search.OrderBy);

                if (orderBy == null) throw new QueryException("Results must be ordered by a valid requested object property. This property cannot exist in an internal object.");

                orderByClause = $" order by [{orderBy.Name}] {(!string.IsNullOrEmpty(search.SortOrder) ? search.SortOrder : SortOrders.Ascending)} {((search.Rows.HasValue && search.Offset.HasValue) ? setPaginatedFetch(search.Offset.Value, search.Rows.Value) : string.Empty)}";
            }

            var whereClause = $" where{(clauseMap["or"].Length > 0 ? $" ({clauseMap["or"]} ) and" : "")}{clauseMap["and"]}";
            var countRows = queryData($"{countSelect} {joins} {whereClause}\n");
            var resultRows = queryData($"{resultSelect} {joins} {whereClause} {orderByClause}");

            var results = new SearchResults<T>
            {
                Matches = QueryUtility.GetInt32FromRow(countRows.First(), "total"),
                Results = map<T>(resultRows, table)
            };

            return results;
        }

        /// <summary>
        /// Returns all objects of the requested type from a database
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <returns>Collection of requested objects</returns>
        public IEnumerable<T> List<T>()
        {
            return list<T>(false);
        }

        /// <summary>
        /// Returns all objects of the requested type from a database
        /// in subsets intended for a paginated grid or list of data
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <returns>Collection of requested objects</returns>
        /// <param name="offset">current page in zero-based format</param>
        /// <param name="rows">number of rows to fetch for the list</param>
        public IEnumerable<T> List<T>(int offset, int rows)
        {
            return list<T>(true, offset, rows);
        }

        /// <summary>
        /// Retrieves objects from a database based on the provided ids
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <param name="ids">Ids of the requested objects</param>
        /// <returns>Collection of requested objects</returns>
        public IEnumerable<T> Get<T>(List<object> ids)
        {
            var entity = Activator.CreateInstance<T>();
            var table = SchemaHandler.GetTableSchema(entity);
            var primaryKeys = getPrimaryKeys(table);

            if (primaryKeys == null) throw new QueryException("Get requests must refer to entities with a defined primary key.");
            if (primaryKeys.Count > 1) throw new Exception("Get requests must refer to entities with only one defined primary key.");

            var sql = new StringBuilder();
            var primaryKey = primaryKeys.First();

            sql.Append($"{setSelectClause(table)} where [{primaryKey.Name}] in ( ");

            for (var i = 0; i < ids.Count; i++) sql.Append($"{string.Format(SqlFormat.Format(primaryKey.Type), ids[i])}{((i < ids.Count - 1) ? ", " : " ) ")}");

            var rows = queryData(sql.ToString());

            return map<T>(rows, table);
        }

        /// <summary>
        /// Retrieves an object from a database based on the provided id
        /// </summary>
        /// <typeparam name="T">Type of requested object</typeparam>
        /// <param name="id">Id of the requested object</param>
        /// <returns>Requested object</returns>
        public T Get<T>(object id)
        {
            return Get<T>(new List<object> { id }).First();
        }

        /// <summary>
        /// Saves an instance of an object in a specified database
        /// </summary>
        /// <param name="entity">Object instance</param>
        /// <returns>Populated object</returns>
        public object Create(object entity)
        {
            var table = SchemaHandler.GetTableSchema(entity);
            var command = setCommand(setInsertSql(table));
            var row = _queryBuilder.QueryData(command).First();

            map(entity, table, row);

            return entity;
        }

        /// <summary>
        /// Saves instances of objects in a specified database as a batch transaction
        /// </summary>
        /// <param name="entities">Object instances</param>
        /// <returns>Populated objects</returns>
        public IEnumerable<object> Create(IEnumerable<object> entities)
        {
            var tables = SchemaHandler.GetTableSchemas(entities);
            var sql = new StringBuilder();

            foreach (var table in tables) sql.AppendLine(setInsertSql(table));

            var command = setCommand(sql.ToString());
            var rows = _queryBuilder.QueryData(command);

            for (var i = 0; i < rows.Count; i++) map(entities.ElementAt(i), tables[i], rows[i]);

            return entities;
        }

        /// <summary>
        /// Updates an instance of an object in a specified database
        /// </summary>
        /// <param name="entity">Object instance</param>
        public void Update(object entity)
        {
            var table = SchemaHandler.GetTableSchema(entity);
            var sql = setUpdateSql(table);

            modifyData(sql);
        }

        /// <summary>
        /// Updates instances of objects in a specified database as a batch transaction
        /// </summary>
        /// <param name="entities">Object instances</param>
        public void Update(IEnumerable<object> entities)
        {
            var tables = SchemaHandler.GetTableSchemas(entities);
            var sql = new StringBuilder();

            foreach (var table in tables) sql.AppendLine(setUpdateSql(table));

            modifyData(sql.ToString());
        }

        /// <summary>
        /// Deletes an instance of an object in a specified database
        /// </summary>
        /// <param name="entity">Object instance</param>
        public void Delete(object entity)
        {
            var table = SchemaHandler.GetTableSchema(entity);
            var primaryKeys = getPrimaryKeys(table);

            if (primaryKeys == null || !primaryKeys.Any()) throw new QueryException("Delete requests must refer to entities with at least one primary key defined.");

            var sql = new StringBuilder();

            sql.Append($"delete from {table.Name} where {setPKClause(primaryKeys)}");

            modifyData(sql.ToString());
        }

        #region Helper Methods

        private static IEnumerable<T> map<T>(IEnumerable<Row> rows, SQL.Table table)
        {
            var initializer = Activator.CreateInstance<T>();
            var properties = initializer.GetType().GetProperties();
            var output = new List<T>();

            foreach (var row in rows)
            {
                var instance = Activator.CreateInstance<T>();

                foreach (var element in row.GetMap())
                {
                    var column = table.Columns.First(c => c.Name == element.Key);
                    var propertyName = column.PropertyName;
                    var property = properties.First(p => p.Name == propertyName);
                    var value = element.Value;

                    if (column.IsForeignKey)
                    {
                        var refEntityType = property.PropertyType;
                        var refEntity = Activator.CreateInstance(refEntityType);
                        var refTable = SchemaHandler.GetTableSchema(refEntity);
                        var refColumn = refTable.Columns.First(c => c.Name == column.ReferenceColumn);
                        var refProperty = refEntityType.GetProperties().First(p => p.Name == refColumn.PropertyName);

                        refProperty.SetValue(refEntity, element.Value);
                        value = refEntity;
                    }

                    property.SetValue(instance, value);
                }

                var unInitializedProperties = properties.Where(p => p.GetValue(instance) == null);

                foreach (var property in unInitializedProperties)
                {
                    var refEntityType = property.PropertyType;
                    var refEntity = Activator.CreateInstance(refEntityType);

                    property.SetValue(instance, refEntity);
                }

                output.Add(instance);
            }

            return output;
        }

        private IEnumerable<T> list<T>(bool paged, int offet = 0, int fetch = 0)
        {
            var entity = Activator.CreateInstance<T>();
            var table = SchemaHandler.GetTableSchema(entity);

            var sql = new StringBuilder();

            sql.Append(setSelectClause(table));

            if (paged) sql.Append($" {setPaginatedFetch(offet, fetch)}");

            var rows = queryData(sql.ToString());

            return map<T>(rows, table);
        }

        private static void map(object entity, SQL.Table table, Row row)
        {
            var properties = entity.GetType().GetProperties();

            foreach (var element in row.GetMap())
            {
                var column = table.Columns.First(c => c.Name == element.Key);
                var propertyName = column.PropertyName;
                var property = properties.First(p => p.Name == propertyName);

                property.SetValue(entity, element.Value);
            }
        }

        private static string setInsertSql(SQL.Table table)
        {
            var insertSql = new StringBuilder();
            var valuesSql = new StringBuilder();

            insertSql.Append($"insert into [{table.Name}] ( ");
            valuesSql.Append(" values ( ");

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                if (column.IsPrimaryKey && column.SequenceType != SequenceTypes.Manual) continue;

                insertSql.Append($"[{column.Name}]{((i < table.Columns.Count - 1) ? ", " : " ) ")}");
                valuesSql.Append($"{string.Format(SqlFormat.Format(column.Type), column.Value)}{((i < table.Columns.Count - 1) ? ", " : " ) ")}");
            }

            var sql = new StringBuilder();

            sql.Append(insertSql);
            sql.Append(valuesSql);

            var primaryKeys = getPrimaryKeys(table);

            if (primaryKeys == null) return sql.ToString();

            var autoIncrementingPKs = primaryKeys.Where(pk => pk.SequenceType == SequenceTypes.Automatic).ToList();

            if (!autoIncrementingPKs.Any()) return sql.ToString();

            sql.Append("\nselect ");

            for (var i = 0; i < autoIncrementingPKs.Count; i ++) sql.AppendFormat(" max([{0}]) as [{0}]{1}", autoIncrementingPKs[i].Name, (i < autoIncrementingPKs.Count - 1) ? ", " : string.Empty);

            sql.Append($" from {table.Name} ");

            return sql.ToString();
        }

        private static string setUpdateSql(SQL.Table table)
        {
            var primaryKeys = getPrimaryKeys(table);

            if (primaryKeys == null || !primaryKeys.Any()) throw new QueryException("Update requests must refer to entities with at least one primary key defined.");

            var columns = table.Columns.Where(c => !c.IsPrimaryKey && !c.CreateOnly).ToList();
            var sql = new StringBuilder();

            sql.Append($"update [{table.Name}] set ");

            for (var i = 0; i < columns.Count; i++) sql.Append($"[{columns[i].Name}] = {string.Format(SqlFormat.Format(columns[i].Type), columns[i].Value)}{((i < columns.Count - 1) ? ", " : string.Empty)} ");

            sql.Append(setPKClause(primaryKeys));

            return sql.ToString();
        }

        private static string setSelectClause(SQL.Table table)
        {
            var sql = new StringBuilder();

            sql.Append("select ");

            for (var i = 0; i < table.Columns.Count; i++)
                sql.Append($"[{table.Columns.ElementAt(i).Name}]{((i < table.Columns.Count - 1) ? ", " : string.Empty)}");

            sql.Append($" from [{table.Name}] ");

            return sql.ToString();
        }
        
        private static string setPKClause(IReadOnlyList<SQL.Column> primaryKeys)
        {
            var sql = new StringBuilder();

            for (var i = 0; i < primaryKeys.Count; i++) sql.Append($" {((i == 0) ? " where " : " and ")}[{primaryKeys[i].Name}] = {string.Format(SqlFormat.Format(primaryKeys[i].Type), primaryKeys[i].Value)}");

            return sql.ToString();
        }

        private static string setPKSelect(IReadOnlyList<SQL.Column> primaryKeys)
        {
            var sql = new StringBuilder();

            for (var i = 0; i < primaryKeys.Count; i++)
                sql.Append($" [{primaryKeys[i].Name}]{(i < primaryKeys.Count - 1 ? "," : string.Empty)} ");

            return sql.ToString();
        }

        private static string setJoin(string objectName, SQL.Table table)
        {
            var column = table.Columns.First(c => c.PropertyName == objectName);

            if (!column.IsForeignKey) throw new QueryException($"A property {objectName} must be a foreign key for creating a proper join clause.");

            return $"join [{column.ReferenceTable}] on [{column.ReferenceTable}].[{column.ReferenceColumn}] = [{table.Name}].[{column.Name}]";
        }

        private static List<SQL.Column> getPrimaryKeys(SQL.Table table)
        {
            return table.Columns.Where(c => c.IsPrimaryKey).ToList();
        }

        private static string setPaginatedFetch(int offset, int rows)
        {
            if (rows < 1) throw new QueryException("Paginated searches and list must specify a valid number of rows (one or more) to fetch.");

            return $"offset {offset} rows fetch next {rows} rows only";
        }

        private IEnumerable<Row> queryData(string sql)
        {
            var command = setCommand(sql);

            return _queryBuilder.QueryData(command);
        }

        private void modifyData(string sql)
        {
            var command = setCommand(sql);

            _queryBuilder.ModifyData(command);
        }

        private static SqlCommand setCommand(string sql, string database = null)
        {
            ConnectionStringSettings settings;

            if (string.IsNullOrEmpty(database))
            {
                var connectionStrings = ConfigurationManager.ConnectionStrings.Count;

                settings = connectionStrings > 0 ? ConfigurationManager.ConnectionStrings[connectionStrings - 1] : null;
            }
            else
            {
                settings = ConfigurationManager.ConnectionStrings[database];
            }

            if (settings == null) throw new QueryException("No valid database connections found.");

            var connection = new SqlConnection(settings.ConnectionString);

            return new SqlCommand(sql, connection);
        }

        #endregion
    }
}