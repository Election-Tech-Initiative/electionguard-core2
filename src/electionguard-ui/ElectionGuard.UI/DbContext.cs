using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI;

public static class DbContext
{
    private const string DbHostKey = "DbAddress";
    private const string DbPasswordKey = "DbPassword";
    private const string DbConnectionStringKey = "DbConnection";

    private const string localhost = "127.0.0.1";

    public static string DbHost => Preferences.Get(DbHostKey, localhost);
    public static string DbPassword = Preferences.Get(DbPasswordKey, string.Empty);
    public static string DbConnection = Preferences.Get(DbConnectionStringKey, string.Empty);
}
