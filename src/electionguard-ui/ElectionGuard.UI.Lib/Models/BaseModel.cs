using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;


namespace ElectionGuard.UI.Lib.Models
{
    public class BaseModel<T> : ObservableObject
    {
        private static string? _table = null;

        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        public BaseModel(string table)
        {
            _table ??= table;
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        }

        public static void Save(T data)
        {
            var db = DbService.GetDb();
            var users = db.GetCollection<T>(_table);
            users.InsertOne(data);
        }
        public static List<T> GetAll()
        {
            var db = DbService.GetDb();
            var data = db.GetCollection<T>(_table);
            var item = data.Find<T>(new BsonDocument());
            var list = item.ToList();
            return list;
        }

        public static T? GetById(string id)
        {
            var db = DbService.GetDb();
            var data = db.GetCollection<T>(_table);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var item = data.Find<T>(filter);
            var list = item.ToList();
            return list.FirstOrDefault();
        }

        public static T? GetByName(string name)
        {
            var db = DbService.GetDb();
            var data = db.GetCollection<T>(_table);
            var filter = Builders<T>.Filter.Eq("Name", name);
            var item = data.Find<T>(filter);
            var list = item.ToList();
            return list.FirstOrDefault();
        }

    }
}