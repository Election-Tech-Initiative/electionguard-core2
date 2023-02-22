﻿namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// Representation of a Schnorr proof
/// </summary>
public class SchnorrProof : DisposableBase
{
    /// <summary>
    /// k in the spec
    /// </summary>
    public ElementModP PublicKey { get; init; }

    /// <summary>
    /// h in the spec
    /// </summary>
    public ElementModP Commitment { get; init; }

    /// <summary>
    /// c in the spec
    /// </summary>
    public ElementModQ Challenge { get; init; }

    /// <summary>
    /// u in the spec
    /// </summary>
    public ElementModQ Response { get; init; }

    public ProofUsage Usage = ProofUsage.SecretValue;

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        PublicKey.Dispose();
        Commitment.Dispose();
        Challenge.Dispose();
        Response.Dispose();
    }

    public bool IsValid()
    {
        /*
            """
            Check validity of the `proof` for proving possession of the private key corresponding
            to `public_key`.

            :return: true if the transcript is valid, false if anything is wrong
            """
         */
        var k = PublicKey;
        var h = Commitment;
        var u = Response;
        var validPublicKey = k.IsValidResidue();
        var inBoundsH = h.IsInBounds();
        var inBoundsU = u.IsInBounds();

        using var c = BigMath.HashElems(k, h);
        using var gp = BigMath.GPowP(u);
        using var pp = BigMath.PowModP(k, c);
        using var mp = BigMath.MultModP(h, pp);

        var validChallenge = c.Equals(Challenge);
        var validProof = gp.Equals(mp);

        var success = validPublicKey && inBoundsH && inBoundsU && validChallenge && validProof;
        if (success is false)
        {
            //log_warning(
            //    "found an invalid Schnorr proof: %s",
            //    str(
            //            {
            //    "in_bounds_h": in_bounds_h,
            //                "in_bounds_u": in_bounds_u,
            //                "valid_public_key": valid_public_key,
            //                "valid_challenge": valid_challenge,
            //                "valid_proof": valid_proof,
            //                "proof": self,
            //            }
            //        ),
            //    )
        }

        return success;
    }
    public SchnorrProof()
    {
        var keyPair = ElGamalKeyPair.FromSecret(BigMath.RandQ());
        var randomSeed = BigMath.RandQ();
        PublicKey = keyPair.PublicKey;
        Commitment = BigMath.GPowP(randomSeed);
        Challenge = BigMath.HashElems(PublicKey, Commitment);
        Response = BigMath.APlusBMulCModQ(randomSeed, keyPair.SecretKey, Challenge);
    }

    /// <summary>
    /// Create a new instance of a Schnorr proof.
    /// </summary>
    /// <param name="keyPair"></param>
    /// <param name="randomSeed"></param>
    public SchnorrProof(ElGamalKeyPair keyPair, ElementModQ randomSeed)
    {
        PublicKey = keyPair.PublicKey;
        Commitment = BigMath.GPowP(randomSeed);
        Challenge = BigMath.HashElems(PublicKey, Commitment);
        Response = BigMath.APlusBMulCModQ(randomSeed, keyPair.SecretKey, Challenge);
    }
}
