using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Users
/// </summary>
public class UserService : BaseDatabaseService<User>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "users";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public UserService() : base(_collection, nameof(User)) { }
}
