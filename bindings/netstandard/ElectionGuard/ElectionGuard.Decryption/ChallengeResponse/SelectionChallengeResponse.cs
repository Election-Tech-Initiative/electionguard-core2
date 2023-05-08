
using System.Diagnostics;
using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ChallengeResponse;

public class SelectionChallengeResponse
{
    /// <summary>
    /// The object id of the selection.
    /// </summary>
    public string ObjectId { get; init; }

    public string GuardianId { get; init; }

    // the sequence order of the guardian
    public ulong SequenceOrder { get; init; }

    public ElementModQ Response { get; init; }

    public SelectionChallengeResponse(
        IElectionGuardian guardian,
        SelectionChallenge challenge,
        ElementModQ response)
    {
        ObjectId = challenge.ObjectId;
        SequenceOrder = guardian.SequenceOrder;
        GuardianId = guardian.GuardianId;
        Response = response;
    }

    public bool IsValid(
        ICiphertextSelection selection,
        AccumulatedSelection accumulated,
        SelectionShare share,
        SelectionChallenge challenge,
        CiphertextElectionContext context
        )
    {
        if (!ObjectId.Equals(selection.ObjectId)
            || !ObjectId.Equals(share.ObjectId)
            || !ObjectId.Equals(challenge.ObjectId)
            || !ObjectId.Equals(accumulated.ObjectId))
        {
            Console.WriteLine($"SelectionChallengeResponse: Invalid object id: \n {ObjectId} \n {selection.ObjectId} \n {share.ObjectId} \n {challenge.ObjectId} \n {accumulated.ObjectId}");
            return false;
        }
        var mbar_i = BigMath.PowModP(share.Share, challenge.Coefficient);

        Console.WriteLine($"SelectionChallengeResponse: IsValid: \n {accumulated.Commitment} \n");
        return IsValid(
            selection.Ciphertext,
            share.Commitment,
            challenge.Challenge,
            context.ElGamalPublicKey,
            mbar_i); // share.Share
    }

    public bool IsValid(
        ElGamalCiphertext ciphertext,
        ElGamalCiphertext commitment,
        ElementModQ challenge,
        ElementModP publicKey,
        ElementModP mBar)
    {
        using var recomputedCommitment = this.ComputeCommitment(
            ciphertext,
            challenge,
            publicKey,
            mBar);

        Console.WriteLine($"SelectionChallengeResponse: IsValid: \n {recomputedCommitment} \n {commitment}");

        return recomputedCommitment.Equals(commitment);
    }
}
