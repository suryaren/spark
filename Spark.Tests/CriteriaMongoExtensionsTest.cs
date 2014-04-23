﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hl7.Fhir.Model;
using Hl7.Fhir.Search;
using Spark.Search;
using M = MongoDB.Driver.Builders;
using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Driver;

namespace Spark.Tests
{
    [TestClass]
    public class CriteriaMongoExtensionsTest
    {
        private IMongoQuery createSimpleQuery(Query query) // without chains
        {
            IMongoQuery resourceFilter = query.ResourceFilter();
            var criteria = query.Criteria.Select(c => Criterium.Parse(c));
            IMongoQuery criteriaFilter = M.Query.And(criteria.Select(c => c.ToFilter(query.ResourceType)));
            return M.Query.And(resourceFilter, criteriaFilter);
        }

        private void AssertQueriesEqual(string expected, string actual)
        {
            Assert.AreEqual(expected.Replace(" ", String.Empty), actual.Replace(" ", String.Empty));
        }

        [TestMethod]
        public void TestNumberQuery()
        {
            var query = new Query().For("ImagingStudy").AddParameter("size", "10");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"ImagingStudy\", \"size\" : \"10\" }", mongoQuery.ToString());
        }

        [TestMethod]
        public void TestStringQueryDefault() //default is partial from start
        {
            var query = new Query().For("Patient").AddParameter("name", "Teun");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"Patient\", \"name\" : /^Teun/i }", mongoQuery.ToString());
        }

        [TestMethod]
        public void TestStringQueryExact()
        {
            var query = new Query().For("Patient").AddParameter("name:exact", "Teun");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"Patient\", \"name\" : \"Teun\" }", mongoQuery.ToString());
        }

        [TestMethod]
        public void TestStringQueryText()
        {
            var query = new Query().For("Patient").AddParameter("name:text", "Teun");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"Patient\", \"namesoundex\" : /^Teun/ }", mongoQuery.ToString());
        }

        [TestMethod]
        public void TestStringQueryChoiceExact()
        {
            var query = new Query().For("Patient").AddParameter("name:exact", "Teun,Truus");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"Patient\", \"name\" : { \"$in\" : [ \"Teun\" , \"Truus\" ] }}", mongoQuery.ToString());
        }

        [TestMethod]
        public void TokenWithCodeAndNamespaceSucceeds()
        {
            var query = new Query().For("DiagnosticReport").AddParameter("name", "TestNS|TestCode");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"DiagnosticReport\" , \"name\": { \"$elemMatch\": { \"system\": \"TestNS\" , \"code\": \"TestCode\"}}}", mongoQuery.ToString());
        }

        [TestMethod]
        public void TokenWithJustCodeSucceeds()
        {
            var query = new Query().For("DiagnosticReport").AddParameter("name", "TestCode");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"DiagnosticReport\" , \"name.code\": \"TestCode\"}", mongoQuery.ToString());
        }

        [TestMethod]
        public void TokenWithCodeAndExplicitlyNoNamespaceSucceeds()
        {
            var query = new Query().For("DiagnosticReport").AddParameter("name", "|TestCode");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"DiagnosticReport\", \"name\" : { \"$elemMatch\": { \"system\": { \"$exists\":false } , \"code\" : \"TestCode\"}}}", mongoQuery.ToString());
        }

        [TestMethod]
        public void TokenWithCodeAndTextModifierSucceeds()
        {
            var query = new Query().For("DiagnosticReport").AddParameter("name:text", "|TestCode");
            var mongoQuery = createSimpleQuery(query);

            Assert.IsNotNull(mongoQuery);
            AssertQueriesEqual("{ \"internal_level\" : 0, \"internal_resource\" : \"DiagnosticReport\",\"$or\":[{\"name_text\":/TestCode/i},{\"name.display\":/TestCode/i}]}", mongoQuery.ToString());
        }
    }
}