using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace ElectionGuard.UI.Lib.Models;

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
