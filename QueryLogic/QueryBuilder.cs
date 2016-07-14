using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using REF = QueryLogic.Reference;

namespace QueryLogic
{
    /// <summary>
    /// Provides methods for retrieving database rows and data sets
    /// </summary>
    public class QueryBuilder : IQueryBuilder
    {
        /// <summary>
        /// Executes a stored procedure which returns data from a single select
        /// </summary>
        /// <param name="command">Database command</param>
        /// <returns>Row maps with retrieved data</returns>
        /// <exception cref="QueryException">SQL execution error</exception>
        /// <exception cref="QueryException">Unable to execute command</exception>
        public Rows QueryData(IDbCommand command)
        {
            var rows = new Rows();

            try
            {
                command.Connection.Open();

                var dataReader = command.ExecuteReader();
                var metadata = setResultSchema(dataReader);

                while (dataReader.Read()) getRowData(dataReader, metadata, rows);
                
                dataReader.Close();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                throw formatError(ex, command);
            }

            return rows;
        }

        /// <summary>
        /// Executes a stored procedure which returns data from a multi-select
        /// </summary>
        /// <param name="command">SQL command</param>
        /// <returns>Dictionary of row maps with retrieved data</returns>
        /// <exception cref="QueryException">SQL execution error</exception>
        /// <exception cref="QueryException">Unable to execute command</exception>
        public DataMap QueryDataSet(IDbCommand command)
        {
            var dataMap = new DataMap();
            
            try
            {
                command.Connection.Open();

                var dataReader = command.ExecuteReader();

                do
                {
                    var metadata = setResultSchema(dataReader);
                    var Rows = new Rows();

                    while (dataReader.Read()) getRowData(dataReader, metadata, Rows);

                    dataMap.Add(Rows);
                } 
                while (dataReader.NextResult());

                dataReader.Close();
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                throw formatError(ex, command);
            }

            return dataMap;
        }

        /// <summary>
        /// Executes a non-query stored procedure
        /// </summary>
        /// <param name="command">SQL command</param>
        /// <returns>Row map with populated out parameters and/or number of affected rows</returns>
        /// <exception cref="QueryException">SQL execution error</exception>
        /// <exception cref="QueryException">Provided command type is not supported</exception>
        public Row ModifyData(IDbCommand command)
        {
            var row = new Row();
            var outputParamNames = getOutParameterNames(command);
            
            try
            {
                command.Connection.Open();
                
                row.Add("rows_affected", command.ExecuteNonQuery());
                
                if (outputParamNames.Any()) setOutputParameterValues(row, command, outputParamNames);
                
                command.Connection.Close();
            }
            catch (Exception ex)
            {
                throw formatError(ex, command);
            }

            return row;
        }

        #region Helper Methods

        private static REF.Metadata setResultSchema(IDataReader dataReader)
        {
            var metadata = new REF.Metadata();
            var schema = dataReader.GetSchemaTable();

            if (!schema.IsValid() || schema == null) return metadata;
            
            foreach (var dataRow in schema.Rows.Cast<DataRow>()) metadata.Columns.Add(new REF.Column(dataRow["ColumnName"].ToString(), dataRow["DataType"].ToString(), metadata.Columns.Count));
            
            return metadata;
        }

        private static void getRowData(IDataReader dataReader, REF.Metadata metadata, ICollection<Row> Rows)
        {
            var result = new Row();

            foreach (var row in metadata.Columns)
            {
                if (dataReader.IsDBNull(row.ColumnIndex))
                {
                    result.Add(row.ColumnName, null);
                }
                else
                {
                    switch (row.ColumnDataType)
                    {
                        case REF.DataTypes.INT16: result.Add(row.ColumnName, dataReader.GetInt16(row.ColumnIndex)); break;
                        case REF.DataTypes.INT32: result.Add(row.ColumnName, dataReader.GetInt32(row.ColumnIndex)); break;
                        case REF.DataTypes.INT64: result.Add(row.ColumnName, dataReader.GetInt64(row.ColumnIndex)); break;
                        case REF.DataTypes.GUID: result.Add(row.ColumnName, dataReader.GetGuid(row.ColumnIndex)); break;
                        case REF.DataTypes.DECIMAL: result.Add(row.ColumnName, dataReader.GetDecimal(row.ColumnIndex)); break;
                        case REF.DataTypes.DATETIME: result.Add(row.ColumnName, dataReader.GetDateTime(row.ColumnIndex)); break;
                        case REF.DataTypes.VARCHAR: result.Add(row.ColumnName, dataReader.GetString(row.ColumnIndex)); break;
                        case REF.DataTypes.BYTE: result.Add(row.ColumnName, dataReader.GetByte(row.ColumnIndex)); break;
                        case REF.DataTypes.BYTECODE: result.Add(row.ColumnName, dataReader.GetRowBytecode(row.ColumnName)); break;
                        case REF.DataTypes.BOOL: result.Add(row.ColumnName, dataReader.GetBoolean(row.ColumnIndex)); break;
                    }
                }
            }

            Rows.Add(result);
        }

        private static List<string> getOutParameterNames(IDbCommand command)
        {
            var parameterNames = new List<string>();

            if (command is SqlCommand)
            {
                parameterNames.AddRange(getSqlParameters(command).Where(x => x.Direction == ParameterDirection.Output).Select(x => x.ParameterName));
            }
            else
            {
                throw new QueryException("Provided command type is not supported.");
            }
            
            return parameterNames;
        }

        private static void setOutputParameterValues(Row row, IDbCommand command, IEnumerable<string> parameterNames)
        {
            if (command is SqlCommand)
            {
                foreach (var paramName in parameterNames)
                {
                    var parameter = command.Parameters[paramName] as SqlParameter;

                    row.Add(paramName.Replace("@", ""), parameter?.Value);
                }
            }
            else
            {
                throw new QueryException("Provided command type is not supported.");
            }
        }

        private static IEnumerable<SqlParameter> getSqlParameters(IDbCommand command)
        {
            return command.Parameters.Cast<SqlParameter>();
        }

        private static QueryException formatError(Exception ex, IDbCommand command)
        {
            return new QueryException($"SQL execution error\ncommand : {command.CommandText} :\nerror : {ex.Message}", ex);
        }

        #endregion
    }
}