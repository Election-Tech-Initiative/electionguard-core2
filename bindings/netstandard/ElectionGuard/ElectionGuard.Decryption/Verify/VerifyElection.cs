using ElectionGuard.Decryption.ElectionRecord;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Verify;

public static class VerifyElection
{
    private static bool IsValidwithKnownSpecDeviations = true;

    public static async Task<VerificationResult> VerifyAsync(
        ElectionRecordData record)
    {
        var results = new List<VerificationResult>();

        // Verification 1 (Parameter validation)
        var parameters = await VerifyElectionParameters(
            record.Constants,
            record.Context,
            record.Manifest
        );
        results.Add(parameters);

        // Verification 2 (Guardian public-key validation)
        var guardianKeys = await VerifyGuardianPublicKeys(
            record.Constants,
            record.Context,
            record.Guardians
        );
        results.Add(guardianKeys);

        // Verification 3 (Election public-key validation)
        var electionKeys = await VerifyElectionPublicKeys(
            record.Constants,
            record.Context,
            record.Guardians
        );
        results.Add(electionKeys);

        // Verification 4 (Correctness of selection encryptions)
        var selectionEncryptions = await VerifySelectionEncryptions(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots
        );
        results.Add(selectionEncryptions);

        // Verification 5 (Adherence to vote limits)
        var voteLimits = await VerifyVoteLimits(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots
        );
        results.Add(voteLimits);

        // Verification 6 (Validation of confirmation codes)
        var confirmationCodes = await VerifyConfirmationCodes(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots
        );
        results.Add(confirmationCodes);

        // Verification 7 (Correctness of ballot aggregation)
        var ballotAggregation = await VerifyBallotAggregation(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots,
            record.EncryptedTally
        );
        results.Add(ballotAggregation);

        // Verification 8 (Correctness of decryptions)
        var decryptions = await VerifyDecryptions(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedTally,
            record.Tally
        );
        results.Add(decryptions);

        // Verification 9 (Validation of correct decryption of tallies)
        var tallyDecryptions = await VerifyTallyDecryptions(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedTally,
            record.Tally
        );
        results.Add(tallyDecryptions);

        return new VerificationResult("Election Verification", results);
    }
    /// <summary>
    /// Verification 1 (Parameter validation)
    /// An ElectionGuard election verifier must verify that it uses the correct version of the 
    /// ElectionGuard specification, that it uses the standard baseline parameters, 
    /// which may be hardcoded, and that the base hash values have been computed correctly.
    /// (1.A) The ElectionGuard specification version used to generate the election record 
    ///       is the same as the ElectionGuard specification version used to verify the election record.
    /// (1.B) The large prime is equal to the large modulus p defined in Section 3.1.1.
    /// (1.C) The small prime is equal to the prime q defined in Section 3.1.1.
    /// (1.D) The cofactor is equal to the value r defined in Section 3.1.1.
    /// (1.E) The generator is equal to the generator g defined in Section 3.1.1.
    /// (1.F) The parameter base hash has been computed correctly as HP =H(ver;0x00,p,q,g)
    ///       using the version byte array ver = 0x76322E302E30 ∥ b(0, 27), which is the UTF-8 encoding
    ///       of the version string “v2.0.0” padded with 0x00-bytes to length 32 bytes.
    /// (1.G) The manifest hash has been computed correctly from the manifest as
    ///       HM =H(HP;0x01,manifest).
    /// (1.H) The base hash has been computed correctly as
    ///       HB =(HP;0x02,HM,n,k).
    /// </summary>
    public static Task<VerificationResult> VerifyElectionParameters(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest
    )
    {
        var results = new List<VerificationResult>();

        // Verification 1.A
        // TODO: E.G. 2.0 - implement
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 1.A: TODO: E.G. 2.0 - implement"));


        // Verification 1.B
        if (constants.P != null && constants.P.Equals(Constants.P))
        {
            results.Add(new VerificationResult(true, "- Verification 1.B: The large prime is equal to the large modulus p defined in Section 3.1.1."));
        }
        else
        {
            results.Add(new VerificationResult(false, $"- Verification 1.B: ElectionConstants.P does not match expected value"));
        }

        // Verification 1.C
        if (constants.Q != null && constants.Q.Equals(Constants.Q))
        {
            results.Add(new VerificationResult(true, "- Verification 1.C: The small prime is equal to the prime q defined in Section 3.1.1."));
        }
        else
        {
            results.Add(new VerificationResult(false, $"- Verification 1.C: ElectionConstants.Q does not match expected value"));
        }

        // Verification 1.D
        if (constants.R != null && constants.R.Equals(Constants.R))
        {
            results.Add(new VerificationResult(true, "- Verification 1.D: The cofactor is equal to the value r defined in Section 3.1.1."));
        }
        else
        {
            results.Add(new VerificationResult(false, $"- Verification 1.D: ElectionConstants.R does not match expected value"));
        }

        // Verification 1.E
        if (constants.G != null && constants.G.Equals(Constants.G))
        {
            results.Add(new VerificationResult(true, "- Verification 1.E: The generator is equal to the generator g defined in Section 3.1.1."));
        }
        else
        {
            results.Add(new VerificationResult(false, $"- Verification 1.E: ElectionConstants.G does not match expected value"));
        }

        // Verification 1.F
        // TODO: E.G. 2.0 - implement
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 1.F: TODO: E.G. 2.0 - implement"));

        // Verification 1.G
        // TODO: E.G. 2.0 - implement
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 1.G: TODO: E.G. 2.0 - implement"));

        // Verification 1.H
        // TODO: E.G. 2.0 - implement
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 1.H: TODO: E.G. 2.0 - implement"));


        return Task.FromResult(new VerificationResult("Verification 1 (Parameter Validation)", results));
    }

    /// <summary>
    /// Verification 2 (Guardian public-key validation)
    /// For each guardian Gi, 1 ≤ i ≤ n, and for each j ∈ Zk, an election verifier must compute the value 
    /// (2.1) hi,j = gvi,j · Kci,j mod p 
    /// and then must confirm the following.
    /// (2.A) The value Ki,j is in Zrp. (A value x is in Zrp if and only if x is an integer 
    ///       such that 0 ≤ x < p and xq mod p = 1 is satisfied.)
    /// (2.B) The value vi,j is in Zq. ( A value x is in Zq if and only if x is an integer 
    ///       such that 0 ≤ x < q.) 
    /// (2.C) The challenge ci,j is correctly computed as ci,j = H (HP ; 0x10, i, j, Ki,j , hi,j ).
    /// </summary>
    public static Task<VerificationResult> VerifyGuardianPublicKeys(
        ElectionConstants constants,
        CiphertextElectionContext context,
        List<ElectionPublicKey> guardians
    )
    {
        var results = new List<VerificationResult>();

        // Verification 2.1
        foreach (var guardian in guardians)
        {
            for (var i = 0; i < guardian.CoefficientProofs.Count; i++)
            {
                var proof = guardian.CoefficientProofs[i];

                using var g_pow_v = BigMath.GPowP(proof.Response);
                using var k_pow_c = BigMath.PowModP(proof.PublicKey, proof.Challenge);
                using var mp = BigMath.MultModP(proof.Commitment, k_pow_c);

                var validCommitment = g_pow_v.Equals(mp);

                results.Add(new VerificationResult(validCommitment, $"- Verification 2.1: Guardian {guardian.ObjectId} commitment {i} is valid"));

                // Verification 2.A
                var inBoundsK = proof.Commitment.IsInBounds();
                results.Add(new VerificationResult(inBoundsK, $"- Verification 2.A: Guardian {guardian.ObjectId} commitment {i} is in bounds"));

                // Verification 2.B
                var inboundsv = proof.Response.IsInBounds();
                results.Add(new VerificationResult(inboundsv, $"- Verification 2.B: Guardian {guardian.ObjectId} response {i} is in bounds"));

                // Verification 2.C
                // TODO: E.G. 2.0 - implement
                results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 2.C: TODO: E.G. 2.0 - implement"));
            }
        }

        return Task.FromResult(new VerificationResult("Verification 2 (Guardian public-key validation)", results));
    }

    /// <summary>
    /// Verification 3 (Election public-key validation)
    /// An election verifier must verify the correct computation of the joint election public key
    /// and extended base hash.
    /// (3.A) The value Ki is in Zrp for all 1 ≤ i ≤ n.
    /// (3.B) K = Qni=1 Ki mod p,
    /// (3.C) HE = H(HB;0x12,K,K1,0,K1,1,...,K1,k−1,K2,0,...,Kn,k−2,Kn,k−1).
    /// </summary>
    public static Task<VerificationResult> VerifyElectionPublicKeys(
        ElectionConstants constants,
        CiphertextElectionContext context,
        List<ElectionPublicKey> guardians
    )
    {
        var results = new List<VerificationResult>();

        // Verification 3.A
        var inBoundsK = context.ElGamalPublicKey.IsInBounds();
        results.Add(new VerificationResult(inBoundsK, $"- Verification 3.A: Election public key is in bounds"));

        // Verification 3.B
        var publicKeys = guardians.Select(k => k.Key).ToList();
        var product = Constants.ONE_MOD_P;
        _ = product.MultModP(publicKeys);
        results.Add(new VerificationResult(product.Equals(context.ElGamalPublicKey), $"- Verification 3.B: Election public key is valid"));

        // Verification 3.C
        // TODO: E.G. 2.0 - implement
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 3.C: TODO: E.G. 2.0 - implement"));

        return Task.FromResult(new VerificationResult("Verification 3 (Election public-key validation)", results));
    }

    /// <summary>
    /// Verification 4 (Correctness of selection encryptions)
    /// For each selectable option on each cast ballot, an election verifier must compute the values
    /// (4.1) aj =gvj ·αcj mod p for all 0 ≤ j ≤ R,
    /// (4.2) bj =Kwj ·βcj mod p,where wj =(vj −jcj) mod q for all 0 ≤ j ≤ R, 
    /// (4.3) c = H(HE;0x21,K,α,β,a0,b0,a1,b1,...,aR,bR),
    ///       where R is the option selection limit. An election verifier must then confirm the following:
    /// (4.A) The given values α and β are in the set Zrp.
    ///       (A value x is in Zrp if and only if x is an integer such that 0 ≤ x < p and xq mod p = 1.)
    /// (4.B) The given values cj each satisfy 0 ≤ cj <2256 for all 0 ≤ j ≤ R.
    /// (4.C) The given values vj are each in the set Zq for all 0 ≤ j ≤ R.
    ///       (A value x is in Zq if and only if x is an integer such that 0 ≤ x < q.)
    /// (4.D) The equation c=(c0+c1+···+cR) mod q is satisfied.
    /// </sumary>
    public static Task<VerificationResult> VerifySelectionEncryptions(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> ballots
    )
    {
        var results = new List<VerificationResult>();

        foreach (var ballot in ballots)
        {
            foreach (var contest in ballot.Contests)
            {
                foreach (var selection in contest.Selections)
                {
                    // Verification 4.1
                }
            }
        }

        return Task.FromResult(new VerificationResult("Verification 4 (Correctness of selection encryptions)", results));
    }

    /// <summary>
    /// Verification 5 (Adherence to vote limits)
    /// For each contest on each cast ballot, an election verifier must compute the contest totals 
    /// (5.1) α ̄ = Qi αi mod p,
    /// (5.2) β ̄ = Qi βi mod p,
    /// where the (αi,βi) represent all possible selections for the contest, as well as the values
    /// (5.3) aj =gvj ·α ̄cj modpforall0≤j≤L,
    /// (5.4) bj =Kwj ·β ̄cj modp,wherewj =(vj −jcj)modqforall0≤j≤L, (5.5) c = H(HE;0x21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL),
    /// where L is the contest selection limit. An election verifier must then confirm the following:
    /// (5.A) The given values αi and βi are each in Zrp.
    /// (5.B) The given values cj each satisfy 0 ≤ cj < 2256.
    /// (5.C) Thegivenvaluesvj areeachinZq forall0≤j≤L. (5.D) Theequationc=(c0+c1+···+cL)modqissatisfied.
    /// </summary>
    public static Task<VerificationResult> VerifyVoteLimits(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> ballots
    )
    {
        var results = new List<VerificationResult>();

        foreach (var ballot in ballots)
        {
            foreach (var contest in ballot.Contests)
            {
                // Verification 5.1
            }
        }

        return Task.FromResult(new VerificationResult("Verification 5 (Adherence to vote limits)", results));
    }

    /// <summary>
    /// Verification 6 (Validation of confirmation codes)
    /// An election verifier must confirm the following for each ballot B.
    /// (6.A) The contest hash χl for the contest with contest index l for all 1 ≤ l ≤ mB has been correctly
    /// computed from the contest selection encryptions (αi,βi) as
    /// χl = H(HE;0x23,l,K,α1,β1,α2,β2 ...,αm,βm).
    /// (6.B) The ballot confirmation code H(B) has been correctly computed from the contest hashes and if specified in the election manifest file from the additional byte array Baux as
    /// H(B) = H(HE;0x24,χ1,χ2,...,χmB ,Baux).
    /// An election verifier must also verify the following.
    /// (6.C) There are no duplicate confirmation codes, i.e. among the set of submitted (cast and chal- lenged) ballots, no two have the same confirmation code.
    /// Additionally, if the election manifest file specifies a hash chain, an election verifier must confirm the following for each voting device.
    /// (6.D) The initial hash code H0 satisfies H0 = H(HE;0x24,Baux,0) and Baux,0 contains the unique voting device information.
    /// (6.E) For all 1 ≤ j ≤ l, the additional input byte array used to compute Hj = H(Bj) is equal to
    /// Baux,j = H(Bj−1) ∥ Baux,0.
    /// (6.F) The final additional input byte array is equal to Baux = H(Bl ) ∥ Baux,0 ∥ b(“CLOSE”, 5) and
    /// H(Bl) is the final confirmation code on this device.
    /// (6.G) The closing hash is correctly computed as H = H(HE;0x24,Baux).
    /// </summary>
    public static Task<VerificationResult> VerifyConfirmationCodes(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> ballots
    )
    {
        var results = new List<VerificationResult>();

        foreach (var ballot in ballots)
        {
            // Verification 6.A
        }

        return Task.FromResult(new VerificationResult("Verification 6 (Validation of confirmation codes)", results));
    }

    /// <summary>
    /// Verification 7 (Correctness of ballot aggregation)
    /// An election verifier must confirm for each option in each contest in the election manifest that the aggregate encryption (A, B) satisfies
    /// (7.A) A = (Qj αj) mod p, 
    /// (7.B) B = (Qj βj) mod p,
    /// where the (αj,βj) are the corresponding encryptions on all cast ballots in the election record.
    /// </summary>
    public static Task<VerificationResult> VerifyBallotAggregation(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> ballots,
        CiphertextTallyRecord tally
    )
    {
        var results = new List<VerificationResult>();



        return Task.FromResult(new VerificationResult("Verification 7 (Correctness of ballot aggregation)", results));
    }

    /// <summary>
    /// Verification 8 (Correctness of decryptions)
    /// For each option in each contest on each tally, an election verifier must compute the values
    /// (8.1) M = B · T −1 mod p, 
    /// (8.2) a=gv ·Kc modp, 
    /// (8.3) b=Av ·Mc modp.
    /// An election verifier must then confirm the following:
    /// (8.A) The given value v is in the set Zq.
    /// (8.B) The challenge value c satisfies c = H(HE;0x30,K,A,B,a,b,M).
    /// </summary>
    public static Task<VerificationResult> VerifyDecryptions(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        CiphertextTallyRecord encryptedTally,
        PlaintextTally decryptedTally
    )
    {
        var results = new List<VerificationResult>();
        return Task.FromResult(new VerificationResult("Verification 8 (Correctness of decryptions)", results));
    }

    /// <summary>
    /// Verification 9 (Validation of correct decryption of tallies)
    /// An election verifier must confirm the following equations for each option in each contest in the election manifest.
    /// (9.A) T = Kt mod p.
    /// An election verifier must also confirm that the text labels listed in the election record tallies match the corresponding text labels in the election manifest. For each contest in a decrypted tally, an election verifier must confirm the following.
    /// (9.B) The contest text label occurs as a contest label in the list of contests in the election manifest.
    /// (9.C) For each option in the contest, the option text label occurs as an option label for the contest
    /// in the election manifest.
    /// (9.D) For each option text label listed for this contest in the election manifest, the option label
    /// occurs for a option in the decrypted tally contest.
    /// An election verifier must also confirm the following.
    /// (9.E) For each contest text label that occurs in at least one submitted ballot, that contest text label occurs in the list of contests in the corresponding tally.
    /// </summary>
    public static Task<VerificationResult> VerifyTallyDecryptions(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        CiphertextTallyRecord encryptedTally,
        PlaintextTally decryptedTally
    )
    {
        var results = new List<VerificationResult>();
        return Task.FromResult(new VerificationResult("Verification 9 (Validation of correct decryption of tallies)", results));
    }

}
