using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ChallengeResponse;

/// <summary>
/// functions to validate the challenge responses.
/// </summary>
public static class ValidateChallengeResponseExtensions
{
    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data.
    ///
    /// equations (63) and (64) in the spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElGamalCiphertext ciphertext,
        ElementModQ challenge,
        Dictionary<string, ElectionPublicKey> guardians,
        ElementModP m_i)
    {
        return self.ComputeCommitment(
            ciphertext.Pad,
            challenge,
            guardians,
            m_i);
    }

    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data.
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
    /// equations (63) and (64) in the spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElementModP ciphertextPad,
        ElementModQ challenge,
        Dictionary<string, ElectionPublicKey> guardians,
        ElementModP m_i)
    {
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
    /// equations (63) and (64) in the spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElementModP ciphertextPad,
        ElementModQ challenge,
        ElementModP commitmentOffset,
        ElementModP m_i)
    {
        // 𝑎𝑖 = 𝑔^𝑣𝑖 • 𝐾'^𝑐𝑖 mod 𝑝
        using var gvi = BigMath.GPowP(self.Response); // 𝑔^𝑣𝑖

        using var Kc = BigMath.PowModP(commitmentOffset, challenge); // 𝐾'^𝑐𝑖
        using var aprime = BigMath.MultModP(gvi, Kc); // 𝑎𝑖 = 𝑔^𝑣𝑖 • 𝐾'^𝑐𝑖

        // 𝑏𝑖 = 𝐴^𝑣𝑖 • 𝑀𝑖^𝑐𝑖 mod 𝑝
        using var avi = BigMath.PowModP(ciphertextPad, self.Response); // 𝐴^𝑣𝑖
        using var Mc = BigMath.PowModP(m_i, challenge); // 𝑀𝑖^𝑐𝑖
        using var bprime = BigMath.MultModP(avi, Mc); // 𝑏𝑖 = 𝐴^𝑣𝑖 • 𝑀𝑖^𝑐𝑖

        return new ElGamalCiphertext(aprime, bprime);
    }
}
