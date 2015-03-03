﻿/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Spark.Core;
using Spark.Store;


namespace Spark
{
    public class MetaStore : IMetaStore
    {
        private MongoDatabase database;
        private MongoCollection collection;

        public MetaStore()
        {
            database = DependencyCoupler.Inject<MongoDatabase>();
            IFhirStore store = DependencyCoupler.Inject<IFhirStore>();
            collection = database.GetCollection(Collection.RESOURCE);
        }

        public List<ResourceStat> GetResourceStats()
        {
            var stats = new List<ResourceStat>();
            List<string> names = Hl7.Fhir.Model.ModelInfo.SupportedResources;

            foreach(string name in names)
            {
                IMongoQuery query = Query.EQ(Field.TYPENAME, name);
                long count = collection.Count(query);
                stats.Add(new ResourceStat() { ResourceName = name, Count = count });
            }
            return stats;
        }
    }

   

   

    

}