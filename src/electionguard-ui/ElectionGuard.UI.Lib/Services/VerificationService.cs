using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for backup verifications
/// </summary>
public class VerificationService : BaseDatabaseService<ElectionPartialKeyVerification>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public VerificationService() : base(_collection) { }

    /// <summary>
    /// Gets a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<List<ElectionPartialKeyVerification>?> GetAllByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(Constants.KeyCeremonyId, keyCeremonyId);
    }

}
