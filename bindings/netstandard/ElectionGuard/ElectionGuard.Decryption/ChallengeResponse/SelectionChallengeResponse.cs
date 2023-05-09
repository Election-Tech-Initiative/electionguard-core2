
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

    // 𝑣𝑖 in the spec
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
        ElectionPublicKey guardian,
        SelectionShare share,
        SelectionChallenge challenge)
    {
        if (!ObjectId.Equals(selection.ObjectId)
            || !ObjectId.Equals(share.ObjectId)
            || !ObjectId.Equals(challenge.ObjectId))
        {
            Console.WriteLine($"SelectionChallengeResponse: Invalid object id: \n {ObjectId} \n {selection.ObjectId} \n {share.ObjectId} \n {challenge.ObjectId}");
            return false;
        }

        Console.WriteLine($"SelectionChallengeResponse: Commitment: \n {share.Commitment} \n");
        return IsValid(
            selection.Ciphertext,
            share.Commitment,
            challenge.Challenge,
            guardian.CoefficientCommitments,
            share.Share);
    }

    public bool IsValid(
        ElGamalCiphertext ciphertext,
        ElGamalCiphertext commitment,
        ElementModQ challenge,
        List<ElementModP> coefficientCommitments,
        ElementModP m_i)
    {
        using var recomputedCommitment = this.ComputeCommitment(
            ciphertext.Pad,
            challenge,
            coefficientCommitments,
            SequenceOrder,
            m_i);

        Console.WriteLine($"SelectionChallengeResponse: recomputedCommitment: \n {recomputedCommitment}");

        return recomputedCommitment.Equals(commitment);
    }
}
