using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Lib.Services
{
    public static class DbService
    {

        //private static readonly string ApplicationName = "ElectionGuard.UI";
        private static readonly int DefaultPort = 27017;
//        private static readonly string DefaultHost = "localhost";
        private static readonly string DefaultDatabase = "ElectionGuardDb";
        private static readonly string DefaultUsername = "root";

        private static string DbHost = string.Empty;
        private static string DbPassword = string.Empty;
        private static MongoClient client = new();

        public static void Init(string host, string password)
        {
            DbHost = host;
            DbPassword = password;

            client = new MongoClient($"mongodb://{DefaultUsername}:{DbPassword}@{DbHost}:{DefaultPort}/{DefaultDatabase}?authSource=admin&keepAlive=true&poolSize=30&autoReconnect=true&socketTimeoutMS=360000&connectTimeoutMS=360000");
        }

        public static IMongoDatabase GetDb()
        {
            return client.GetDatabase(DefaultDatabase);
        }

        public static bool Verify()
        {
            var db = GetDb();
            return db.ListCollectionNames().ToList().Count > 0;
        }
    }
}
