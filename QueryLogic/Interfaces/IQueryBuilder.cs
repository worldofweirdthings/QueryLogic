using System.Data;

namespace QueryLogic
{
    /// <summary>
    /// Specifies methods for retrieving database rows and data sets
    /// </summary>
    public interface IQueryBuilder
    {
        /// <summary>
        /// Executes a stored procedure which returns data from a single select
        /// </summary>
        /// <param name="command">Database command</param>
        /// <returns>Row maps with retrieved data</returns>
        /// <exception cref="QueryException">SQL execution error</exception>
        /// <exception cref="QueryException">Unable to execute command</exception>
        Rows QueryData(IDbCommand command);

        /// <summary>
        /// Executes a stored procedure which returns data from a multi-select.
        /// </summary>
        /// <param name="command">SQL command</param>
        /// <returns>Dictionary of row maps with retrieved data</returns>
        /// <exception cref="QueryException">SQL execution error</exception>
        /// <exception cref="QueryException">Unable to execute command</exception>
        DataMap QueryDataSet(IDbCommand command);

        /// <summary>
        /// Executes a non-query stored procedure.
        /// </summary>
        /// <param name="command">SQL command</param>
        /// <returns>Row map with populated out parameters and/or number of affected rows</returns>
        /// <exception cref="QueryException">SQL execution error</exception>
        /// <exception cref="QueryException">Provided command type is not supported</exception>
        Row ModifyData(IDbCommand command);
    }
}