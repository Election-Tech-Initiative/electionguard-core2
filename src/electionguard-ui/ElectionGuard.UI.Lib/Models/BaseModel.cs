using CommunityToolkit.Mvvm.ComponentModel;
using ElectionGuard.UI.Lib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ElectionGuard.UI.Lib.Models;

public class BaseModel : ObservableObject
{
    [JsonIgnore]
    public static string table = string.Empty;

    public string GetTable() => table;

    public static List<T> GetAll<T>()
    {
        var list = new List<T>();
        var db = DbService.GetDb();
        var users = db.GetCollection<T>(T.GetTable());
        return list;
    }

}
