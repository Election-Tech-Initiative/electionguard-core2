using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for LagrangeCoefficientsService
/// </summary>
public class LagrangeCoefficientsService : BaseDatabaseService<LagrangeCoefficientsRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableTallies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public LagrangeCoefficientsService() : base(_collection, nameof(LagrangeCoefficientsRecord)) { }

    /// <summary>
    /// Gets Lagrange Coefficients for an election
    /// </summary>
    /// <param name="tallyId">tally id to search for</param>
    public async Task<LagrangeCoefficientsRecord?> GetByTallyIdAsync(string tallyId)
    {
        return await GetByFieldAsync(DbConstants.TallyId, tallyId);
    }

}
