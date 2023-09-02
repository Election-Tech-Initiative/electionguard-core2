using ElectionGuard.Decryption.ChallengeResponse;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Extensions;

/// <summary>
/// functions to validate the challenge responses.
/// </summary>
public static class ComputeCommitmentExtensions
{
    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data.
    ///
    /// This overload accepts an array of guardians that must match the number of
    /// guardians that participated in the key ceremony.
    ///
    /// equations (63) and (64) in the spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElGamalCiphertext ciphertext,
        ElementModQ challenge,
        CiphertextElectionContext context,
        Dictionary<string, ElectionPublicKey> guardians,
        ElementModP m_i)
    {
        return self.ComputeCommitment(
            ciphertext.Pad,
            challenge,
            context,
            guardians,
            m_i);
    }

    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data.
    ///
    /// This overload accepts an array of guardians that must match the number of
    /// guardians that participated in the key ceremony.
    /// 
    /// equations (63) and (64) in the spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElGamalCiphertext ciphertext,
        ElementModQ challenge,
        ElementModP commitmnetOffset,
        ElementModP m_i)
    {
        return self.ComputeCommitment(
            ciphertext.Pad,
            challenge,
            commitmnetOffset,
            m_i);
    }

    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data.
    ///
    /// equations (74) and (75) in the v2.0.0 spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElementModP ciphertextPad,
        ElementModQ challenge,
        CiphertextElectionContext context,
        Dictionary<string, ElectionPublicKey> guardians,
        ElementModP m_i)
    {
        if (guardians.Count != (int)context.NumberOfGuardians)
        {
            throw new ArgumentException($"guardians.Count != context.NumberOfGuardians: {guardians.Count} != {context.NumberOfGuardians}");
        }

        // Π 𝐾^𝑖^m mod 𝑝
        var commitmentOffset = guardians.ComputeCommitmentOffset(self.SequenceOrder);
        return self.ComputeCommitment(
            ciphertextPad,
            challenge,
            commitmentOffset,
            m_i);
    }

    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data.
    ///
    ///  equations (74) and (75) in the v2.0.0 spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionShare self,
        ElementModP ciphertextPad,
        ElementModQ challenge,
        ElementModP commitmentOffset,
        ElementModQ response)
    {
        return ComputeCommitment(
            ciphertextPad,
            challenge,
            commitmentOffset,
            response,
            self.Share);
    }

    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data.
    ///
    ///  equations (74) and (75) in the v2.0.0 spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElementModP ciphertextPad,
        ElementModQ challenge,
        ElementModP commitmentOffset,
        ElementModP m_i)
    {
        return ComputeCommitment(
            ciphertextPad,
            challenge,
            commitmentOffset,
            self.Response,
            m_i);
    }

    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data.
    ///
    ///  equations (74) and (75) in the v2.0.0 spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        ElementModP ciphertextPad,
        ElementModQ challenge,
        ElementModP commitmentOffset,
        ElementModQ response,
        ElementModP m_i)
    {
        // 𝑎𝑖 = 𝑔^𝑣𝑖 • 𝐾'^𝑐𝑖 mod 𝑝
        using var gvi = BigMath.GPowP(response); // 𝑔^𝑣𝑖

        using var Kc = BigMath.PowModP(commitmentOffset, challenge); // 𝐾'^𝑐𝑖
        using var aprime = BigMath.MultModP(gvi, Kc); // 𝑎𝑖 = 𝑔^𝑣𝑖 • 𝐾'^𝑐𝑖

        // 𝑏𝑖 = 𝐴^𝑣𝑖 • 𝑀𝑖^𝑐𝑖 mod 𝑝
        using var avi = BigMath.PowModP(ciphertextPad, response); // 𝐴^𝑣𝑖
        using var Mc = BigMath.PowModP(m_i, challenge); // 𝑀𝑖^𝑐𝑖
        using var bprime = BigMath.MultModP(avi, Mc); // 𝑏𝑖 = 𝐴^𝑣𝑖 • 𝑀𝑖^𝑐𝑖

        return new ElGamalCiphertext(aprime, bprime);
    }
}
