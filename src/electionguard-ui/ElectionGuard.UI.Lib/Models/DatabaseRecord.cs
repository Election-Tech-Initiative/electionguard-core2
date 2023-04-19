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
    public class DatabaseRecord : ObservableObject
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string DataType { get; set; }

        public bool SoftDeleted { get; set; }

        public DatabaseRecord(string dataType)
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            DataType = dataType;
            SoftDeleted = false;
        }
    }
}
