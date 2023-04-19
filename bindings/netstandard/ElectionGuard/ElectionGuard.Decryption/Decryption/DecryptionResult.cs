using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>   
/// The result of a decryption operation
/// </summary>
public class DecryptionResult
{
    /// <summary>
    /// The id of the tally
    /// </summary>
    public string TallyId { get; init; }

    /// <summary>
    /// Whether the decryption was successful
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The message if the decryption was not successful
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The result of the decryption
    /// </summary>
    public PlaintextTally? Tally { get; set; }

    /// <summary>
    /// The result of decrypting the spoiled ballots
    /// </summary>
    public List<PlaintextTallyBallot>? ChallengedBallots { get; set; }

    public List<CiphertextBallot>? SpoiledBallots { get; set; }

    public DecryptionResult(string tallyId, bool isValid = true)
    {
        TallyId = tallyId;
        IsValid = isValid;
    }

    public DecryptionResult(
        string tallyId,
        PlaintextTally result)
    {
        TallyId = tallyId;
        IsValid = true;
        Tally = result;
    }

    public DecryptionResult(
        string tallyId,
        PlaintextTally result,
        List<PlaintextTallyBallot> challengedBallots,
        List<CiphertextBallot> spoiledBallots)
    {
        TallyId = tallyId;
        IsValid = true;
        Tally = result;
        ChallengedBallots = challengedBallots;
        SpoiledBallots = spoiledBallots;
    }

    public DecryptionResult(
        string tallyId,
        string message)
    {
        TallyId = tallyId;
        IsValid = false;
        Message = message;
    }

    public static implicit operator bool(DecryptionResult self)
    {
        return self.IsValid;
    }

    public static implicit operator string(DecryptionResult self)
    {
        return self.Message ?? string.Empty;
    }

    public static implicit operator PlaintextTally?(DecryptionResult self)
    {
        return self.Tally;
    }

    public static implicit operator List<PlaintextTallyBallot>?(DecryptionResult self)
    {
        return self.ChallengedBallots;
    }

    public static implicit operator List<CiphertextBallot>?(DecryptionResult self)
    {
        return self.SpoiledBallots;
    }
}
