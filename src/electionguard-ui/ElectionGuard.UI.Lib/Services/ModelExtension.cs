using ElectionGuard.UI.Lib.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Lib.Services
{
    public static class ModelExtension
    {
        public static void Save<T>(this T data)
        {
            var db = DbService.GetDb();
            var users = db.GetCollection<T>((data as BaseModel)?.GetTable());
            users.InsertOne(data);
        }
    }
}
