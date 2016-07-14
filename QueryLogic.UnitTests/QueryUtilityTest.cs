using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryLogic.ORM;

namespace QueryLogic.UnitTests
{
    [TestClass]
    public class QueryUtilityTest
    {
        [TestMethod]
        public void AddInputParameterExtensionTest()
        {
            var testCommand = new SqlCommand();

            testCommand.AddParameter("test_param", 0);

            Assert.AreEqual("@test_param", testCommand.Parameters[0].ParameterName);
            Assert.AreEqual(0, testCommand.Parameters[0].Value);
        }

        [TestMethod]
        public void AddOutputParameterExtensionTest()
        {
            var testCommand = new SqlCommand();

            testCommand.OutParameter("test_param", SqlDbType.Int);

            Assert.AreEqual("@test_param", testCommand.Parameters[0].ParameterName);
            Assert.AreEqual(ParameterDirection.Output, testCommand.Parameters[0].Direction);
        }

        [TestMethod]
        public void IsDBTableValidExtensionTest()
        {
            var dataTable = new DataTable();
            var validation = dataTable.IsValid();

            Assert.IsFalse(validation);
        }

        [TestMethod]
        public void ToIdStringExtensionTest()
        {
            var ids = new List<int> { 1, 2, 3, 4, 5 };
            var idList = ids.ToIdString();

            Assert.AreEqual("1,2,3,4,5,", idList);
        }
        
        [TestMethod]
        public void AreSearchTermsNullOrEmptyTest()
        {
            var validSearchTerms = new List<SearchTerm> 
            { 
                new SearchTerm(false, "test.field.name", "like", "test")
            };

            var nullSearchTerms = new Search();

            var validSearchTermValidation = isNullOrEmpty(validSearchTerms);
            var emptySearchTermsValidation = isNullOrEmpty(new List<SearchTerm>());
            var nullSearchTermsValidation = isNullOrEmpty(nullSearchTerms.SearchTerms);

            Assert.IsFalse(validSearchTermValidation);
            Assert.IsTrue(emptySearchTermsValidation);
            Assert.IsTrue(nullSearchTermsValidation);
        }

        [TestMethod]
        public void GetInt16FromRowValidTest()
        {
            var row = new Row(new RowMap
            {
                { "test_row", (short) 15 }
            });
            
            var data = QueryUtility.GetInt16FromRow(row, "test_row");

            Assert.AreEqual(15, data);
            Assert.AreEqual(typeof(short), data.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetInt16FromRowInvalidTest()
        {
            var row = new Row();

            QueryUtility.GetInt16FromRow(row, "test_row");
        }

        [TestMethod]
        public void GetInt32FromRowValidTest()
        {
            var row = new Row(new RowMap
            {
                { "test_row", 15 }
            });

            var data = QueryUtility.GetInt32FromRow(row, "test_row");

            Assert.AreEqual(15, data);
            Assert.AreEqual(typeof(int), data.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetInt32FromRowInvalidTest()
        {
            var row = new Row();

            QueryUtility.GetInt32FromRow(row, "test_row");
        }

        [TestMethod]
        public void GetInt64FromRowValidTest()
        {
            var row = new Row(new RowMap
            {
                { "test_row", 15L }
            });

            var data = QueryUtility.GetInt64FromRow(row, "test_row");

            Assert.AreEqual(15, data);
            Assert.AreEqual(typeof(long), data.GetType());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetInt64FromRowInvalidTest()
        {
            var row = new Row();

            QueryUtility.GetInt64FromRow(row, "test_row");
        }

        [TestMethod]
        public void GetGuidFromRowValidTest()
        {
            var guid = Guid.NewGuid();
            var row = new Row(new RowMap
            {
                { "test_row", guid }
            });

            var data = QueryUtility.GetGuidFromRow(row, "test_row");

            Assert.AreEqual(guid, data);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetGuidFromRowInvalidTest()
        {
            var row = new Row();

            QueryUtility.GetGuidFromRow(row, "test_row");
        }

        [TestMethod]
        public void GetStringFromRowValidTest()
        {
            var row = new Row(new RowMap
            {
                { "test_row", "test" }
            });
            
            var data = QueryUtility.GetStringFromRow(row, "test_row");

            Assert.AreEqual("test", data);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetStringFromRowInvalidTest()
        {
            var row = new Row();
            
            QueryUtility.GetStringFromRow(row, "test_row");
        }

        [TestMethod]
        public void GetDecimalFromRowValidTest()
        {
            var decimalValue = Convert.ToDecimal(1.5m);
            var row = new Row(new RowMap
            {
                { "test_row", decimalValue }
            });

            var data = QueryUtility.GetDecimalFromRow(row, "test_row");

            Assert.AreEqual(decimalValue, data);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetDecimalFromRowInvalidTest()
        {
            var row = new Row();
            
            QueryUtility.GetDecimalFromRow(row, "test_row");
        }

        [TestMethod]
        public void GetDateTimeFromRowValidTest()
        {
            var dateTimeValue = DateTime.Now;
            var row = new Row(new RowMap
            {
                { "test_row", dateTimeValue }
            });

            var data = QueryUtility.GetDateTimeFromRow(row, "test_row");

            Assert.AreEqual(dateTimeValue, data);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void GetDateTimeFromRowInalidTest()
        {
            var row = new Row();
            
            QueryUtility.GetDateTimeFromRow(row, "test_row");
        }

        [TestMethod]
        public void GetBytecodeFromRowValidTest()
        {
            var byteArray = new byte[] { 0xFF };
            var row = new Row(new RowMap
            {
                { "test_row", byteArray }
            });
            
            var data = QueryUtility.GetBytecodeFromRow(row, "test_row");

            Assert.AreEqual(byteArray[0], data[0]);
        }

        [TestMethod]
        public void GetGenericFromRowTest()
        {
            var guid = Guid.NewGuid();
            var row = new Row(new RowMap
            {
                { "test_row", guid }
            });

            var data = QueryUtility.GetGenericFromRow<Guid>(row, "test_row");

            Assert.IsNotNull(data);
            Assert.AreEqual(guid, data);
        }

        [TestMethod]
        public void GetNullableFromRowTest()
        {
            var row = new Row(new RowMap
            {
                { "test_row", new Guid?() }
            });

            var data = QueryUtility.GetNullableFromRow<Guid>(row, "test_row");

            Assert.IsNull(data);
        }

        [TestMethod]
        public void NewCommandTest()
        {
            var command = QueryUtility.NewCommand("testproc", "SqlTestDB");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(IDbCommand));
            
            Assert.AreEqual("testproc", command.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, command.CommandType);
        }

        [TestMethod]
        public void DefaultNewCommandTest()
        {
            var command = QueryUtility.NewCommand("testproc");

            Assert.IsNotNull(command);
            Assert.IsInstanceOfType(command, typeof(IDbCommand));

            Assert.AreEqual("testproc", command.CommandText);
            Assert.AreEqual(CommandType.StoredProcedure, command.CommandType);
        }

        [TestMethod]
        [ExpectedException(typeof(QueryException))]
        public void InvalidNewCommandTest()
        {
            QueryUtility.NewCommand("testproc", "wrongkey");
        }

        #region Helpers

        private static bool isNullOrEmpty(IEnumerable<SearchTerm> searchTerms)
        {
            return !searchTerms.Any();
        }

        #endregion
    }
}