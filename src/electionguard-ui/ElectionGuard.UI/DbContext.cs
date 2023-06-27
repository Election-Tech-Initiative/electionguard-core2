using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI;

public static class DbContext
{
    public const string DbHostKey = "DbAddress";
    public const string DbPasswordKey = "DbPassword";
    public const string DbConnectionStringKey = "DbConnection";

    private const string localhost = "127.0.0.1";

    public static string DbHost
    { 
        get => Preferences.Get(DbHostKey, localhost);
        set => Preferences.Set(DbHostKey, value.Trim());
    }
    public static string DbPassword { 
        get => Preferences.Get(DbPasswordKey, string.Empty);
        set => Preferences.Set(DbPasswordKey, value.Trim());
    }
    public static string DbConnection {
        get => Preferences.Get(DbConnectionStringKey, string.Empty);
        set => Preferences.Set(DbConnectionStringKey, value.Trim());
    }

    public static bool IsValid()
    {
        return !string.IsNullOrEmpty(DbConnection) || !string.IsNullOrEmpty(DbPassword);
    }
}
