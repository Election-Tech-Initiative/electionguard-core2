using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Elections
/// </summary>
public class ElectionService : BaseDatabaseService<Election>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "elections";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public ElectionService() : base(_collection) { }
}
