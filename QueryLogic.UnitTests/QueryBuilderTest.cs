using System;
using System.Text;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QueryLogic.UnitTests
{
    [TestClass]
    public class QueryBuilderTest
    {
        [TestMethod]
        public void RunQueryGetAllTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.GET_DATA);
            
            command.AddParameter("test_key", 0);

            var results = queryBuilder.QueryData(command);

            Assert.IsNotNull(results[0]["sqltest_int32"]);
            Assert.IsNotNull(results[0]["sqltest_string"]);
            Assert.IsNotNull(results[0]["sqltest_decimal"]);
            Assert.IsNotNull(results[0]["sqltest_datetime"]);
        }

        [TestMethod]
        public void RunQueryGetAllOverloadTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.GET_DATA, ProcedureNames.DATA_SOURCE);
            
            command.AddParameter("test_key", 0);

            var results = queryBuilder.QueryData(command);

            Assert.IsNotNull(results[0]["sqltest_int32"]);
            Assert.IsNotNull(results[0]["sqltest_string"]);
            Assert.IsNotNull(results[0]["sqltest_decimal"]);
            Assert.IsNotNull(results[0]["sqltest_datetime"]);
        }

        [TestMethod]
        public void RunDataSetQueryTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.GET_MANIFEST_DATA);
            
            command.AddParameter("getaddl", 1);
            command.AddParameter("getmore", 1);

            var results = queryBuilder.QueryDataSet(command);

            Assert.AreEqual(results.Count, 3);
            Assert.IsNotNull(results.First()[0]["sqltestdata_value"]);
            Assert.IsNotNull(results.Next()[0]["addlsqltestdata_value"]);
            Assert.IsNotNull(results.Next()[0]["moresqltestdata_value"]);
        }
        
        [TestMethod]
        [ExpectedException(typeof(QueryException))]
        public void RunQueryInvalidTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.GET_DATA);
            
            queryBuilder.QueryData(command);
        }

        [TestMethod]
        public void AddNewEntryTest()
        {
            var queryBuilder = new QueryBuilder();
            var addCommand = QueryUtility.NewCommand(ProcedureNames.ADD_DATA);

            addCommand.AddParameter("test_string", "sql test string 2");
            addCommand.AddParameter("test_decimal", Convert.ToDecimal(4.30));
            addCommand.AddParameter("test_datetime", DateTime.Parse("2012-12-21 23:59:59.463"));

            var result = queryBuilder.ModifyData(addCommand);

            Assert.AreEqual(1, (int) result["rows_affected"]);

            var getCommand = QueryUtility.NewCommand(ProcedureNames.GET_DATA);
            var maxId = getMaxTestEntityKey();
            
            getCommand.AddParameter("test_key", maxId);
            
            var results = queryBuilder.QueryData(getCommand);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(maxId, results[0]["sqltest_int32"]);
            Assert.AreEqual("sql test string 2", results[0]["sqltest_string"]);
            Assert.AreEqual(Convert.ToDecimal(4.30), results[0]["sqltest_decimal"]);
            Assert.AreEqual(DateTime.Parse("2012-12-21 23:59:59.463"), results[0]["sqltest_datetime"]);
        }

        [TestMethod]
        public void UpdateExistingEntryTest()
        {
            var queryBuilder = new QueryBuilder();
            var maxId = getMaxTestEntityKey();
            var updateCommand = QueryUtility.NewCommand(ProcedureNames.UPDATE_DATA);

            updateCommand.AddParameter("test_key", maxId);
            updateCommand.AddParameter("test_string", "sql test string 2.5");
            updateCommand.AddParameter("test_decimal", Convert.ToDecimal(4.35));
            updateCommand.AddParameter("test_datetime", DateTime.Parse("2012-12-21 23:59:59.463"));

            var result = queryBuilder.ModifyData(updateCommand);

            Assert.AreEqual((int) result["rows_affected"], 1);

            var getCommand = QueryUtility.NewCommand(ProcedureNames.GET_DATA);
            
            getCommand.AddParameter("test_key", maxId);
            
            var results = queryBuilder.QueryData(getCommand);

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(maxId, results[0]["sqltest_int32"]);
            Assert.AreEqual("sql test string 2.5", results[0]["sqltest_string"]);
            Assert.AreEqual(Convert.ToDecimal(4.35), results[0]["sqltest_decimal"]);
            Assert.AreEqual(DateTime.Parse("2012-12-21 23:59:59.463"), results[0]["sqltest_datetime"]);
        }

        [TestMethod]
        public void DeleteExistingEntryTest()
        {
            var queryBuilder = new QueryBuilder();
            var maxId = getMaxTestEntityKey();
            var deleteCommand = QueryUtility.NewCommand(ProcedureNames.DELETE_DATA);
            
            deleteCommand.AddParameter("test_key", maxId);
            
            var result = queryBuilder.ModifyData(deleteCommand);

            Assert.AreEqual(1, (int) result["rows_affected"]);

            var getCommand = QueryUtility.NewCommand(ProcedureNames.GET_DATA);
            
            getCommand.AddParameter("test_key", maxId);
            
            var results = queryBuilder.QueryData(getCommand);

            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public void RunNonQueryWithOutputTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.GET_DATA_OUTPUT);

            command.AddParameter("input", 5);
            command.OutParameter("output", SqlDbType.Int);

            var result = queryBuilder.ModifyData(command);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(5, result["output"]);
        }

        [TestMethod]
        [ExpectedException(typeof(QueryException))]
        public void RunNonQueryIgnoreOutputTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.GET_DATA_OUTPUT);
            
            command.AddParameter("input", 5);
            queryBuilder.ModifyData(command);
        }

        [TestMethod]
        [ExpectedException(typeof(QueryException))]
        public void RunNonQueryInvalidTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.UPDATE_DATA);
            
            queryBuilder.ModifyData(command);
        }

        [TestMethod]
        public void AddBinaryDataTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.ADD_BINARY_DATA);
            
            command.AddParameter("bytecode", Encoding.ASCII.GetBytes("sql test bytes"));
            
            var result = queryBuilder.ModifyData(command);

            Assert.AreEqual(1, (int) result["rows_affected"]);
        }

        [TestMethod]
        public void GetBinaryDataTest()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.GET_BINARY_DATA);
            
            command.AddParameter("binaryid", 1);
            
            var results = queryBuilder.QueryData(command);

            Assert.AreEqual(2, results[0].Count);
            Assert.IsNotNull(results[0]["sqlbinarytest_id"]);
            Assert.IsNotNull(results[0]["sqlbinarytest_bytecode"]);
        }

        private static int getMaxTestEntityKey()
        {
            var queryBuilder = new QueryBuilder();
            var command = QueryUtility.NewCommand(ProcedureNames.GET_MAX_DATA_KEY);
            var results = queryBuilder.QueryData(command);

            return (int) results[0]["max_id"];
        }

        private struct ProcedureNames
        {
            public const string DATA_SOURCE = "SqlTestDB";
            public const string GET_DATA = "gettestdata";
            public const string GET_MANIFEST_DATA = "getmanifestedsqltestdata";
            public const string ADD_DATA = "addtestentry";
            public const string UPDATE_DATA = "updatetestentry";
            public const string DELETE_DATA = "deletetestentry";
            public const string GET_MAX_DATA_KEY = "getmaxid";
            public const string GET_DATA_OUTPUT = "getsqloutput";
            public const string GET_BINARY_DATA = "getbinarydata";
            public const string ADD_BINARY_DATA = "addbinarydata";
        }
    }
}