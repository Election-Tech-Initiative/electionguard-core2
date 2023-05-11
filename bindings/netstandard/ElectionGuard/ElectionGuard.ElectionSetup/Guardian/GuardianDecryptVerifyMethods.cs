using ElectionGuard.Ballot;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// methods for decrypting
/// </summary>
public partial class Guardian
{
    /// <summary>
    /// Create a commitment for a specific selection.
    /// The commitment is used as part of the validation process
    /// to prove that the guardian computed their share correctly.
    ///
    // (𝑎𝑖, 𝑏𝑖) = (𝑔^𝑢𝑖 mod 𝑝, 𝐴^𝑢𝑖 mod 𝑝).
    /// </summary>
    public ElGamalCiphertext CreateCommitment(ICiphertextSelection selection)
    {
        if (AllGuardianKeysReceived is false)
        {
            throw new InvalidOperationException(
                "All guardian keys must be received before decrypting.");
        }

        // create a nonce for the selection
        using var nonces = NonceForSelection(selection);
        using var u = nonces.Next();

        using var a = BigMath.GPowP(u);  // 𝑔^𝑢𝑖 mod 𝑝
        using var b = BigMath.PowModP(selection.Ciphertext.Pad, u);  // 𝐴^𝑢𝑖 mod 𝑝

        return new ElGamalCiphertext(a, b);
    }

    /// <summary>
    /// Create a response to a challenge created by an administrator.
    /// The response is used as part of the validation process
    /// to prove that the guardian computed their share correctly.
    ///
    /// The challenge is the guardian-scoped challenge 
    /// that has been adjusted by the lagrange coefficient
    /// as part of Equation (61) in the spec. 𝑐𝑖 = 𝑐 • ω𝑖 mod q
    /// 
    /// Computes:
    /// 𝑣𝑖 = (𝑢𝑖 − 𝑐𝑖𝑃(𝑖)) mod q. Equation (62)
    /// </summary>
    public ElementModQ CreateResponse(
        IElectionSelection selection,
        ElementModQ challenge)
    {
        if (AllGuardianKeysReceived is false)
        {
            throw new InvalidOperationException(
                "All guardian keys must be received before decrypting.");
        }

        // recreate the nonce for the selection
        using var nonces = NonceForSelection(selection);
        using var u = nonces.Next();

        // rehydrate the partial secret if it has not been done already
        if (_myPartialSecretKey is null)
        {
            Console.WriteLine($"CreateResponse: rehydrating partial secret key");
            _ = CombinePrivateKeyShares();
        }

        // 𝑣𝑖 = (𝑢𝑖 − 𝑐𝑖P(𝑖)) mod q. Equation (62)
        using var product = BigMath.MultModQ(challenge, _myPartialSecretKey);
        var v = BigMath.SubModQ(u, product);
        return v;
    }

    /// <summary>
    /// create a nonce specific for the selection that is based on the guardian commitment seed
    /// </summary>
    private Nonces NonceForSelection(IElectionSelection selection)
    {
        using var selectionHash = Hash.HashElems(
            selection.ObjectId, selection.SequenceOrder, selection.DescriptionHash);
        using var hashSeed = Hash.HashElems(_commitmentSeed, selectionHash);

        // TODO: move this magic string to a constant
        return new Nonces(hashSeed, "chaum-pedersen-proof");
    }
}
