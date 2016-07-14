using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using QueryLogic.Constants;

namespace QueryLogic
{
    /// <summary>
    /// Provides utilities and extensions for querying and modifying database information,
    /// as well as basic handling for data extracted from database transactions
    /// </summary>
    public static class QueryUtility
    {
        #region Extension Methods

        /// <summary>
        /// Adds a parameter to a database command.
        /// </summary>
        /// <param name="command">Current database command</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="parameterValue">Value of the parameter</param>
        /// <exception cref="QueryException">Provided command type is not supported</exception>
        public static void AddParameter(this IDbCommand command, string parameterName, object parameterValue)
        {
            if (command is SqlCommand)
            {
                command.Parameters.Add(new SqlParameter($"@{parameterName}", parameterValue ?? DBNull.Value));
            }
            else
            {
                throw new QueryException("Provided command type is not supported.");
            }
        }

        /// <summary>
        /// Adds an out parameter to a database command.
        /// </summary>
        /// <param name="command">Current database command</param>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="defaultValue">Default value of the parameter used to infer the return type</param>
        /// <exception cref="QueryException">Provided command type is not supported</exception>
        public static void OutParameter(this IDbCommand command, string parameterName, object defaultValue)
        {
            if (!(command is SqlCommand)) throw new QueryException("Provided command type is currently not supported.");
            
            var outParameter = new SqlParameter
            {
                ParameterName = $"@{parameterName}",
                Value = defaultValue,
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(outParameter);
        }

        /// <summary>
        /// Turns a collection of integers into a comma-separated list.
        /// </summary>
        /// <param name="ids">List of integers</param>
        /// <returns>Comma-separated list of integers</returns>
        public static string ToIdString(this IEnumerable<int> ids)
        {
            return ids.Aggregate("", (str, id) => str + $"{id},");
        }

        /// <summary>
        /// Attempts to paralellize execution of multiple actions.
        /// </summary>
        /// <param name="actions">Actions to be paralellized</param>
        public static void TryParallelize(this IEnumerable<Action> actions)
        {
            var inputActions = actions?.ToList() ?? new List<Action>();

            switch (inputActions.Count)
            {
                case 0 : return;
                case 1 : inputActions[0].Invoke(); break;
                default : Parallel.ForEach(inputActions, a => a.Invoke()); break;
            }
        }

        /// <summary>
        /// Checks if the retrieved database table schema is not null or empty.
        /// </summary>
        /// <param name="dataTable">Current database table schema</param>
        /// <returns>Table schema is valid</returns>
        public static bool IsValid(this DataTable dataTable)
        {
            return dataTable != null && dataTable.Rows.Count > 0;
        }

        #endregion

        #region Command Builders

        /// <summary>
        /// Creates a new database command object based on the specified provider
        /// </summary>
        /// <param name="commandText">Name of stored procedure to be executed</param>
        /// <param name="sourceName">Name of the connection string in the Web/App.config file</param>
        /// <exception cref="QueryException">No valid connection strings found</exception>
        /// <exception cref="QueryException">A database provider is unsupported</exception>
        /// <returns>New SQL command</returns>
        public static IDbCommand NewCommand(string commandText = null, string sourceName = null)
        {
            var command = buildCommand(commandText, sourceName);
            
            command.CommandType = CommandType.StoredProcedure;
            
            return command;
        }


        /// <summary>
        /// Creates a new database command object for executing inline SQL based on 
        /// the specified provider
        /// </summary>
        /// <param name="commandText">Name of stored procedure to be executed</param>
        /// <param name="sourceName">Name of the connection string in the Web/App.config file</param>
        /// <exception cref="QueryException">No valid connection strings found</exception>
        /// <exception cref="QueryException">A database provider is unsupported</exception>
        /// <returns>New SQL command</returns>
        public static IDbCommand NewInlineSql(string commandText = null, string sourceName = null)
        {
            var command = buildCommand(commandText, sourceName);

            command.CommandType = CommandType.Text;

            return command;
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Retuns an array of bytes from a retrieved column
        /// </summary>
        /// <param name="dataReader">Current Data Reader</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Array of bytes from the retrieved cell</returns>
        public static byte[] GetRowBytes(IDataReader dataReader, string columnName)
        {
            var index = dataReader.GetOrdinal(columnName);
            var size = Convert.ToInt32(dataReader.GetBytes(index, 0, null, 0, 0));
            var buffer = new byte[size];

            dataReader.GetBytes(index, 0, buffer, 0, size);

            return buffer.Where(b => b != 0).ToArray();
        }

        /// <summary>
        /// Checks is there is data for the requested cell, meant for 
        /// populating nullible properties from nullible fields
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Boolean flag</returns>
        public static bool CheckIfCellIsNull(Row row, string columnName)
        {
            return row[columnName] == null;
        }

        /// <summary>
        /// Returns a value of a type specified by the user
        /// </summary>
        /// <typeparam name="T">User defined type</typeparam>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Instance of the user specified type from the retrieved cell</returns>
        public static T GetGenericFromRow<T>(Row row, string columnName)
        {
            return row[columnName] == null ? default(T) : (T)row[columnName];
        }

        /// <summary>
        /// Returns a nullable value of a type specified by the user
        /// </summary>
        /// <typeparam name="T">User defined type</typeparam>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Nullable type and value if any is found</returns>
        public static T? GetNullableFromRow<T>(Row row, string columnName) where T : struct
        {
            return row[columnName] == null ? new T?() : (T) row[columnName];
        }

        /// <summary>
        /// Retrieves a single byte (tinyint) from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Byte from the retrieved cell</returns>
        public static byte GetByteFromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (byte) row[columnName] : new byte();
        }

        /// <summary>
        /// Retrieves a 2 byte integer (smallint) from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Integer from the retrieved cell</returns>
        public static short GetInt16FromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (short) row[columnName] : short.MinValue;
        }

        /// <summary>
        /// Retrieves a 4 byte integer from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Integer from the retrieved cell</returns>
        public static int GetInt32FromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (int) row[columnName] : int.MinValue;
        }

        /// <summary>
        /// Retrieves an 8 byte integer (bigint) from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Integer from the retrieved cell</returns>
        public static long GetInt64FromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (long) row[columnName] : long.MinValue;
        }

        /// <summary>
        /// Retrieves a GUID from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>String from the retrieved cell</returns>
        public static Guid GetGuidFromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (Guid) row[columnName] : Guid.Empty;
        }

        /// <summary>
        /// Retrieves a string from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>String from the retrieved cell</returns>
        public static string GetStringFromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (string) row[columnName] : string.Empty;
        }

        /// <summary>
        /// Retrieves a decimal from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Decimal from the retrieved cell</returns>
        public static decimal GetDecimalFromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (decimal) row[columnName] : 0m;
        }

        /// <summary>
        /// Retrieves a timestamp from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Timestamp from the retrieved cell</returns>
        public static DateTime GetDateTimeFromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (DateTime) row[columnName] : DateTime.MinValue;
        }

        /// <summary>
        /// Retrieves an array of bytes from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Array of bytes from the retrieved cell</returns>
        public static byte[] GetBytecodeFromRow(Row row, string columnName)
        {
            return row[columnName] != null ? (byte[]) row[columnName] : new byte[0];
        }

        /// <summary>
        /// Retrieves a boolean flag from a row map
        /// </summary>
        /// <param name="row">Current row map</param>
        /// <param name="columnName">Name of database column</param>
        /// <returns>Boolean value</returns>
        public static bool GetBooleanFromRow(Row row, string columnName)
        {
            return row[columnName] != null && (bool) row[columnName];
        }

        #endregion

        #region Helper Methods

        internal static IDbCommand buildCommand(string commandText = null, string sourceName = null)
        {
            IDbCommand command = new SqlCommand(commandText);

            var connectionStrings = ConfigurationManager.ConnectionStrings.Count;
            var connectionSettings = string.IsNullOrEmpty(sourceName)
                ? ConfigurationManager.ConnectionStrings[connectionStrings - 1]
                : ConfigurationManager.ConnectionStrings[sourceName];

            if (connectionSettings == null) throw new QueryException("No valid connection strings found.");

            var providerName = connectionSettings.ProviderName ?? DatabaseProviders.MSSQL;

            if (providerName.ToLower().Contains(DatabaseProviders.MSSQL))
                command.Connection = new SqlConnection(connectionSettings.ConnectionString);

            if (providerName.ToLower().Contains(DatabaseProviders.Oracle))
                throw new QueryException("Oracle commands are currently not supported.");

            if (providerName.ToLower().Contains(DatabaseProviders.MySQL))
                throw new QueryException("MySQL commands are currently not supported.");

            return command;
        }

        internal static byte[] GetRowBytecode(this IDataReader dataReader, string columnName)
        {
            return GetRowBytes(dataReader, columnName);
        }
        
        #endregion
    }
}