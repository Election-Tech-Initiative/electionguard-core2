using ElectionGuard.UI.Lib.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Lib.Services
{
    public static class UserExtension
    {
        public static void Save(this User user)
        {
            if (user == null) 
            {
                return;
            } 
            var db = DbService.GetDb();
            var users = db?.GetCollection<User>("users");
            users?.InsertOne(user);
        }
    }
}
