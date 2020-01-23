using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.NetCore
{
    public class MongoDBClient
    {
        public static MongoClient Connect()
        {
            // To directly connect to a single MongoDB server
            // (this will not auto-discover the primary even if it's a member of a replica set)
            //var client = new MongoClient();

            // or use a connection string
            var client = new MongoClient("mongodb://localhost:27017");
            return client;
            // or, to connect to a replica set, with auto-discovery of the primary, supply a seed list of members
            //var client = new MongoClient("mongodb://localhost:27017,localhost:27018,localhost:27019");
        }
        
    }
}
