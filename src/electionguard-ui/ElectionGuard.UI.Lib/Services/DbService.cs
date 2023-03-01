using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace ElectionGuard.UI.Lib.Services;


    public class ComplexTypeSerializer : SerializerBase<object>
{
    public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
        var document = serializer.Deserialize(context, args);

        var bsonDocument = document.ToBsonDocument();

        var result = BsonExtensionMethods.ToJson(bsonDocument);
        return JsonSerializer.Deserialize<HashedElGamalCiphertext>(result)!;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        var jsonDocument = JsonSerializer.Serialize(value);
        var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

        var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
        serializer.Serialize(context, bsonDocument.AsBsonValue);
    }
}

/// <summary>
/// Database service class to access the mongo database
/// </summary>
public static class DbService
{
    private static readonly int DefaultPort = 27017;
    private static readonly string DefaultDatabase = "ElectionGuardDb";
    private static readonly string DefaultUsername = "root";

    private static string DbHost = string.Empty;
    private static string DbPassword = string.Empty;
    private static MongoClient client = new();

    /// <summary>
    /// Initializes the connection to the database server
    /// This will need to be called if the address or the password needs to be changed.
    /// </summary>
    /// <param name="host">The IP address of the database</param>
    /// <param name="password">Password used for the database</param>
    public static void Init(string host, string password)
    {
        DbHost = host;
        DbPassword = password;

        // Create a new MongoClient that uses the user, password, host, port and database names
        client = new MongoClient($"mongodb://{DefaultUsername}:{DbPassword}@{DbHost}:{DefaultPort}/{DefaultDatabase}?authSource=admin&keepAlive=true&poolSize=30&autoReconnect=true&socketTimeoutMS=360000&connectTimeoutMS=360000");

        //BsonSerializer.RegisterSerializer(typeof(HashedElGamalCiphertext), new ComplexTypeSerializer());
    }

    /// <summary>
    /// Gets an interface to the database
    /// </summary>
    /// <returns>Gets the connection to the database</returns>
    public static IMongoDatabase GetDb()
    {
        return client.GetDatabase(DefaultDatabase);
    }

    /// <summary>
    /// Gets a collection from the database
    /// </summary>
    /// <typeparam name="T">Data type to use for the collection</typeparam>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>The found collection</returns>
    public static IMongoCollection<T> GetCollection<T>(string? collectionName)
    {
        var db = GetDb();
        return db.GetCollection<T>(collectionName);
    }

    /// <summary>
    /// Method to verify that the connection to the database is working
    /// </summary>
    /// <returns>True/false if the client is connected to the server</returns>
    public static bool Verify()
    {
        var db = GetDb();
        return db.ListCollectionNames().ToList().Count > 0;
    }
}
