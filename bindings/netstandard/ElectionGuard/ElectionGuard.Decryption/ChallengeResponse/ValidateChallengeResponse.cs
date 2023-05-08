namespace ElectionGuard.Decryption.ChallengeResponse;

public static class ValidateChallengeResponseExtensions
{
    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElGamalCiphertext ciphertext,
        ElementModQ challenge,
        ElementModP elGamalPublicKey,
        ElementModP mBar)
    {
        return self.ComputeCommitment(
            ciphertext.Pad,
            challenge,
            elGamalPublicKey,
            mBar);
    }

    public static ElGamalCiphertext ComputeCommitment(
        this SelectionChallengeResponse self,
        ElementModP ciphertextPad,
        ElementModQ challenge,
        ElementModP elGamalPublicKey,
        ElementModP mBar)
    {
        // 𝑎𝑖 = 𝑔^𝑣𝑖 * 𝐾^𝑐𝑖 mod 𝑝
        var gvi = BigMath.GPowP(self.Response); // 𝑔^𝑣𝑖
        var Kc = BigMath.PowModP(elGamalPublicKey, challenge); // 𝐾^𝑐𝑖
        var aprime = BigMath.MultModP(gvi, Kc); // 𝑎𝑖 = 𝑔^𝑣𝑖 * 𝐾^𝑐𝑖

        // 𝑏𝑖 = 𝐴^𝑣𝑖 * 𝑀𝑖^𝑐𝑖 mod 𝑝
        var avi = BigMath.PowModP(ciphertextPad, self.Response); // 𝐴^𝑣𝑖
        var Mc = BigMath.PowModP(mBar, challenge); // 𝑀𝑖^𝑐𝑖
        var bprime = BigMath.MultModP(avi, Mc); // 𝑏𝑖 = 𝐴^𝑣𝑖 * 𝑀𝑖^𝑐𝑖

        return new ElGamalCiphertext(aprime, bprime);
    }
}
