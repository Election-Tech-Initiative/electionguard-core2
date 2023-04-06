using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Encryption.Ballot;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

public record CiphertextDecryptionContestShare
    : DisposableRecordBase, IElectionContest, IEquatable<CiphertextDecryptionContestShare>
{
    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// The sequence order of the contest
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The hash of the contest description
    /// </summary>
    public ElementModQ DescriptionHash { get; init; }

    public Dictionary<string, CiphertextDecryptionSelectionShare> Selections { get; init; } = default!;

    public CiphertextDecryptionContestShare(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        Dictionary<string, CiphertextDecryptionSelectionShare> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = descriptionHash;
        Selections = selections;
    }

    public CiphertextDecryptionContestShare(IElectionContest contest,
    Dictionary<string, CiphertextDecryptionSelectionShare> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = contest.DescriptionHash;
        Selections = selections;
    }

    public bool IsValid(
        CiphertextBallotContest contest,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (contest.ObjectId != ObjectId)
        {
            return false;
        }

        foreach (var selection in contest.Selections)
        {
            if (!Selections.ContainsKey(selection.ObjectId))
            {
                return false;
            }

            if (!Selections[selection.ObjectId].IsValid(
                selection, guardian, extendedBaseHash))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsValid(
        CiphertextTallyContest contest,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (contest.ObjectId != ObjectId)
        {
            return false;
        }

        foreach (var selection in contest.Selections)
        {
            if (!Selections.ContainsKey(selection.Key))
            {
                return false;
            }

            if (!Selections[selection.Key].IsValid(
                selection.Value, guardian, extendedBaseHash))
            {
                return false;
            }
        }
        return true;
    }
}
