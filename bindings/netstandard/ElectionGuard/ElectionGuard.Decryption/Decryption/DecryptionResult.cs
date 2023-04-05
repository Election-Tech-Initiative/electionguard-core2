using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Decryption;

public class DecryptionResult
{
    /// <summary>
    /// The id of the tally
    /// </summary>
    public string TallyId { get; init; }
    public bool IsValid { get; set; }
    public string? Message { get; set; }

    public PlaintextTally? Tally { get; set; }

    public List<PlaintextTallyBallot>? SpoiledBallots { get; set; }

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
        List<PlaintextTallyBallot> spoiledBallots)
    {
        TallyId = tallyId;
        IsValid = true;
        Tally = result;
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
        return self.SpoiledBallots;
    }
}
