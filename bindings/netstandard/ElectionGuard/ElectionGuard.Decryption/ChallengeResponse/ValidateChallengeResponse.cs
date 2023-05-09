using ElectionGuard.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ChallengeResponse;

public static class ValidateChallengeResponseExtensions
{
    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data
    /// equations (63) and (64) in the spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElGamalCiphertext ciphertext,
        ElementModQ challenge,
        ElectionPublicKey guardian,
        ElementModP m_i)
    {
        return self.ComputeCommitment(
            ciphertext.Pad,
            challenge,
            guardian.CoefficientCommitments,
            guardian.SequenceOrder,
            m_i);
    }

    /// <summary>
    /// Compute the commitment for the selection using 
    /// the publically available data
    /// equations (63) and (64) in the spec
    /// </summary>
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElementModP ciphertextPad,
        ElementModQ challenge,
        List<ElementModP> coefficientCommitments,
        ulong sequenceOrder,
        ElementModP m_i)
    {
        // 𝑎𝑖 = 𝑔^𝑣𝑖 • 𝐾^𝑐𝑖 mod 𝑝
        var gvi = BigMath.GPowP(self.Response); // 𝑔^𝑣𝑖

        // Π 𝐾^𝑖^m mod 𝑝
        using var calculated = new ElementModP(Constants.ONE_MOD_P);
        foreach (var (commitment, index) in coefficientCommitments.WithIndex())
        {
            using var exponent = BigMath.PowModP(sequenceOrder, index);
            using var k_pow_im = BigMath.PowModP(commitment, exponent);
            _ = calculated.MultModP(k_pow_im);
        }

        var Kc = BigMath.PowModP(calculated, challenge); // 𝐾^𝑐𝑖
        var aprime = BigMath.MultModP(gvi, Kc); // 𝑎𝑖 = 𝑔^𝑣𝑖 • 𝐾^𝑐𝑖

        // 𝑏𝑖 = 𝐴^𝑣𝑖 • 𝑀𝑖^𝑐𝑖 mod 𝑝
        var avi = BigMath.PowModP(ciphertextPad, self.Response); // 𝐴^𝑣𝑖
        var Mc = BigMath.PowModP(m_i, challenge); // 𝑀𝑖^𝑐𝑖
        var bprime = BigMath.MultModP(avi, Mc); // 𝑏𝑖 = 𝐴^𝑣𝑖 • 𝑀𝑖^𝑐𝑖

        return new ElGamalCiphertext(aprime, bprime);
    }
}
