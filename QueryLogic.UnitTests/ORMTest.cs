using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryLogic.ORM;

namespace QueryLogic.UnitTests
{
    [TestClass]
    public class ORMTest
    {
        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Strict);
        
        private Mock<IQueryBuilder> _queryBuilder;
        private ISqlHandler _sqlHandler;
        
        [TestInitialize]
        public void Initialize()
        {
            _queryBuilder = _mockRepository.Create<IQueryBuilder>();
            _sqlHandler = new SqlHandler(_queryBuilder.Object);
        }

        [TestMethod]
        public void TestSimpleFind()
        {
            _queryBuilder.SetupSequence(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getSearchCount()).Returns(getBasicEntitySelect());

            var search = new Search();

            search.SearchTerms.Add(new SearchTerm(false, "Name", SearchOperators.Like, "basicentity%"));

            var result = _sqlHandler.Find<BasicEntity>(search);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Results, typeof(List<BasicEntity>));

            Assert.AreEqual(result.Matches, 1);
            Assert.AreEqual(result.Results.ElementAt(0).Id, 5);
            Assert.AreEqual(result.Results.ElementAt(0).Name, "basic entity");
            Assert.AreEqual(result.Results.ElementAt(0).Timestamp, DateTime.Parse("2012-12-21 23:59:59.000"));

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestComplexFind()
        {
            _queryBuilder.SetupSequence(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getSearchCount()).Returns(getComplexEntitySelect());
                        
            var search = new Search
            {
                OrderBy = "Name",
                SortOrder = SortOrders.Ascending,
                Offset = 0,
                Rows = 25                
            };

            search.SearchTerms.Add(new SearchTerm(false, "BasicEntity.Name", SearchOperators.Like, "basicentity%"));

            var result = _sqlHandler.Find<ComplexEntity>(search);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Results, typeof(List<ComplexEntity>));

            Assert.AreEqual(result.Matches, 1);
            Assert.AreEqual(result.Results.ElementAt(0).Id, 5);
            Assert.AreEqual(result.Results.ElementAt(0).Name, "complex entity");
            Assert.AreEqual(result.Results.ElementAt(0).BasicEntity.Id, 1);

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestComplexFindWithOrs()
        {
            _queryBuilder.SetupSequence(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getSearchCount()).Returns(getComplexEntitySelect());

            var search = new Search
            {
                OrderBy = "Name",
                SortOrder = SortOrders.Ascending,
                Offset = 0,
                Rows = 25
            };

            search.SearchTerms.Add(new SearchTerm(true, "Name", SearchOperators.Like, "basicentity%"));
            search.SearchTerms.Add(new SearchTerm(true, "BasicEntity.Name", SearchOperators.Like, "basicentity%"));
            search.SearchTerms.Add(new SearchTerm(false, "Timestamp", SearchOperators.GreaterThan, "'2012-12-21'"));

            var result = _sqlHandler.Find<ComplexEntity>(search);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Results, typeof(List<ComplexEntity>));

            Assert.AreEqual(result.Matches, 1);
            Assert.AreEqual(result.Results.ElementAt(0).Id, 5);
            Assert.AreEqual(result.Results.ElementAt(0).Name, "complex entity");
            Assert.AreEqual(result.Results.ElementAt(0).BasicEntity.Id, 1);

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestSimpleGet()
        {
            _queryBuilder.Setup(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getBasicEntitySelect);

            var result = _sqlHandler.Get<BasicEntity>(5);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BasicEntity));
            
            Assert.AreEqual(result.Id, 5);
            Assert.AreEqual(result.Name, "basic entity");
            Assert.AreEqual(result.Timestamp, DateTime.Parse("2012-12-21 23:59:59.000"));

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestComplexGet()
        {
            _queryBuilder.Setup(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getComplexEntitySelect);

            var result = _sqlHandler.Get<ComplexEntity>(5);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ComplexEntity));

            Assert.AreEqual(result.Id, 5);
            Assert.AreEqual(result.Name, "complex entity");
            Assert.AreEqual(result.BasicEntity.Id, 1);
            Assert.AreEqual(result.Timestamp, DateTime.Parse("2012-12-21 23:59:59.000"));

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestBasicCreate()
        {
            _queryBuilder.Setup(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getBasicCreate);

            var entity = getBasicEntity();
            var result = _sqlHandler.Create(entity) as BasicEntity;

            Assert.IsNotNull(result);
            
            Assert.AreEqual(result.Id, 5);
            Assert.AreEqual(result.Name, "basic entity");
            Assert.AreEqual(result.Timestamp, DateTime.Parse("2012-12-21 23:59:59.000"));

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestComplexCreate()
        {
            _queryBuilder.Setup(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getComplexCreate);

            var entity = getComplexEntity();
            var result = _sqlHandler.Create(entity) as ComplexEntity;

            Assert.IsNotNull(result);
            
            Assert.AreEqual(result.Id, 5);
            Assert.AreEqual(result.Name, "complex entity");
            Assert.AreEqual(result.BasicEntity.Id, 3);

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestBasicBatchCreate()
        {
            _queryBuilder.Setup(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getBasicBatchCreate);

            var entities = new List<BasicEntity>();

            for (var i = 0; i < 3; i++)
            {
                var entity = getBasicEntity();

                entity.Name = $"basic entity batch {(i + 1)}";

                entities.Add(entity);
            }

            var results = _sqlHandler.Create(entities) as List<BasicEntity>;

            Assert.IsNotNull(results);
            
            Assert.AreEqual(results.ElementAt(0).Id, 6);
            Assert.AreEqual(results.ElementAt(0).Name, "basic entity batch 1");
            Assert.AreEqual(results.ElementAt(0).Timestamp, DateTime.Parse("2012-12-21 23:59:59.000"));

            Assert.AreEqual(results.ElementAt(1).Id, 7);
            Assert.AreEqual(results.ElementAt(1).Name, "basic entity batch 2");
            Assert.AreEqual(results.ElementAt(1).Timestamp, DateTime.Parse("2012-12-21 23:59:59.000"));

            Assert.AreEqual(results.ElementAt(2).Id, 8);
            Assert.AreEqual(results.ElementAt(2).Name, "basic entity batch 3");
            Assert.AreEqual(results.ElementAt(2).Timestamp, DateTime.Parse("2012-12-21 23:59:59.000"));

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestComplexBatchCreate()
        {
            _queryBuilder.Setup(x => x.QueryData(It.IsAny<IDbCommand>())).Returns(getComplexBatchCreate);

            var entities = new List<ComplexEntity>();

            for (var i = 0; i < 3; i++)
            {
                var entity = getComplexEntity();

                entity.Name = $"complex entity batch {(i + 1)}";
                entity.BasicEntity.Id = (i + 1);

                entities.Add(entity);
            }

            var results = _sqlHandler.Create(entities) as List<ComplexEntity>;

            Assert.IsNotNull(results);

            Assert.AreEqual(results.ElementAt(0).Id, 6);
            Assert.AreEqual(results.ElementAt(0).Name, "complex entity batch 1");
            Assert.AreEqual(results.ElementAt(0).BasicEntity.Id, 1);

            Assert.AreEqual(results.ElementAt(1).Id, 7);
            Assert.AreEqual(results.ElementAt(1).Name, "complex entity batch 2");
            Assert.AreEqual(results.ElementAt(1).BasicEntity.Id, 2);

            Assert.AreEqual(results.ElementAt(2).Id, 8);
            Assert.AreEqual(results.ElementAt(2).Name, "complex entity batch 3");
            Assert.AreEqual(results.ElementAt(2).BasicEntity.Id, 3);

            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestBasicUpdate()
        {
            _queryBuilder.Setup(x => x.ModifyData(It.IsAny<IDbCommand>())).Returns(new Row());

            var entity = getBasicEntity();

            entity.Id = 1;

            _sqlHandler.Update(entity);
            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestComplexUpdate()
        {
            _queryBuilder.Setup(x => x.ModifyData(It.IsAny<IDbCommand>())).Returns(new Row());

            var entity = getComplexEntity();

            entity.Id = 1;

            _sqlHandler.Update(entity);
            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestBasicBatchUpdate()
        {
            _queryBuilder.Setup(x => x.ModifyData(It.IsAny<IDbCommand>())).Returns(new Row());

            var entities = new List<BasicEntity>();

            for (var i = 0; i < 3; i++)
            {
                var entity = getBasicEntity();

                entity.Id = (i + 1);
                entity.Name = $"basic entity batch {(i + 1)}";

                entities.Add(entity);
            }

            _sqlHandler.Update(entities);
            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void TestComplexBatchUpdate()
        {
            _queryBuilder.Setup(x => x.ModifyData(It.IsAny<IDbCommand>())).Returns(new Row());

            var entities = new List<ComplexEntity>();

            for (var i = 0; i < 3; i++)
            {
                var entity = getComplexEntity();

                entity.Id = (i + 1);
                entity.Name = $"complex entity batch {(i + 1)}";
                entity.BasicEntity.Id = (i + 1);

                entities.Add(entity);
            }

            _sqlHandler.Update(entities);
            _mockRepository.VerifyAll();
        }

        #region Test Entities

        [Table("basicentity")]
        private class BasicEntity
        {
            [PrimaryKey(Sequence = SequenceTypes.Automatic)]
            [Column("basicentity_id")]
            public int? Id { get; set; }

            [Column("basicentity_name")]
            public string Name { get; set; }

            [Column("basicentity_timestamp")]
            public DateTime Timestamp { get; set; }
        }

        [Table("complexentity")]
        private class ComplexEntity
        {
            [PrimaryKey(Sequence = SequenceTypes.Automatic)]
            [Column("complexentity_id")]
            public int? Id { get; set; }

            [Column("complexentity_name")]
            public string Name { get; set; }

            [Column("basicentity_id")]
            [ForeignKey("basicentity", "basicentity_id")]
            public BasicEntity BasicEntity { get; set; }

            [Column("complexentity_timestamp")]
            public DateTime Timestamp { get; set; }
        }

        #endregion

        #region Test Instances

        private BasicEntity getBasicEntity()
        {
            return new BasicEntity
            {
                Id = null,
                Name = "basic entity",
                Timestamp = DateTime.Parse("2012-12-21 23:59:59.000")
            };
        }

        private ComplexEntity getComplexEntity()
        {
            return new ComplexEntity
            {
                Id = null,
                Name = "complex entity",
                BasicEntity = new BasicEntity
                {
                    Id = 3,
                    Name = "basic entity",
                    Timestamp = DateTime.Parse("2012-12-21 23:59:59.000")
                },
                Timestamp = DateTime.Parse("2012-12-21 23:59:59.000")
            };
        }

        #endregion

        #region Test Rows

        private static Rows getBasicEntitySelect()
        {
            var row = new Row(new RowMap
            {
                { "basicentity_id", 5 },
                { "basicentity_name", "basic entity" },
                { "basicentity_timestamp", DateTime.Parse("2012-12-21 23:59:59.000") }
            });
            
            return new Rows { row };
        }

        private static Rows getComplexEntitySelect()
        {
            var row = new Row(new RowMap
            {
                { "complexentity_id", 5 },
                { "complexentity_name", "complex entity" },
                { "basicentity_id", 1 },
                { "complexentity_timestamp", DateTime.Parse("2012-12-21 23:59:59.000") }
            });
            
            return new Rows { row };
        }

        private static Rows getSearchCount()
        {
            return new Rows
            {
                new Row(new RowMap { { "total", 1 } })
            };
        }

        private static Rows getBasicCreate()
        {
            return new Rows
            {
                new Row(new RowMap { { "basicentity_id", 5 } })
            };
        }

        private static Rows getComplexCreate()
        {
            return new Rows
            {
                new Row(new RowMap { { "complexentity_id", 5 } })
            };
        }

        private static Rows getBasicBatchCreate()
        {
            return new Rows
            {
                new Row(new RowMap { { "basicentity_id", 6 } }),
                new Row(new RowMap { { "basicentity_id", 7 } }),
                new Row(new RowMap { { "basicentity_id", 8 } })
            };
        }

        private static Rows getComplexBatchCreate()
        {
            return new Rows
            {
                new Row(new RowMap { { "complexentity_id", 6 } }),
                new Row(new RowMap { { "complexentity_id", 7 } }),
                new Row(new RowMap { { "complexentity_id", 8 } })
            };
        }

        #endregion
    }
}