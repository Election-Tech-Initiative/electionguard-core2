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

        // Verification 4 (Extended base hash validation)
        var extendedBaseHash = await VerifyExtendedBaseHash(
            record.Constants,
            record.Context
        );
        results.Add(extendedBaseHash);

        // Verification 5 (Well-formedness of selection encryptions)
        var selectionEncryptions = await VerifySelectionEncryptions(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots
        );
        results.Add(selectionEncryptions);

        // Verification 6 (Adherence to vote limits)
        var voteLimits = await VerifyVoteLimits(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots
        );
        results.Add(voteLimits);

        // Verification 7 (Validation of confirmation codes)
        var confirmationCodes = await VerifyConfirmationCodes(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots
        );
        results.Add(confirmationCodes);

        // Verification 8 (Correctness of ballot aggregation)
        var ballotAggregation = await VerifyBallotAggregation(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots,
            record.EncryptedTally
        );
        results.Add(ballotAggregation);

        // Verification 9 (Correctness of decryptions)
        var tallyDecryptions = await VerifyTallyDecryptions(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedTally,
            record.Tally
        );
        results.Add(tallyDecryptions);

        // Verification 10 (Validation of correct decryption of tallies)
        var tallyMetadata = await VerifyTallyDecryptionMetadata(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedTally,
            record.Tally
        );
        results.Add(tallyMetadata);

        // Verification 11 (Correctness of decryptions of contest data)
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "Verification 11 (Correctness of decryptions of contest data): TODO: E.G. 2.0 - implement"));

        // Verification 12 (Correctness of decryptions for challenged ballots)
        var ballotDecryptions = await VerifyBallotDecryptions(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots
                .Where(i => i.IsChallenged)
                .ToList(),
            record.ChallengedBallots
        );
        results.Add(ballotDecryptions);

        // Verification 13 (Validation of correct decryption of challenged ballots)
        var ballotDecryptionMetadata = await VerifyBallotDecryptionMetadata(
            record.Constants,
            record.Context,
            record.Manifest,
            record.EncryptedBallots
                .Where(i => i.IsChallenged)
                .ToList(),
            record.ChallengedBallots
        );
        results.Add(ballotDecryptionMetadata);

        // Verification 14 (Correctness of contest data decryptions for challenged ballots)
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "Verification 14 (Correctness of contest data decryptions for challenged ballots): TODO: E.G. 2.0 - implement"));

        // Verification 15 (Well-formedness of selection encryptions in pre-encrypted ballots)
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "Verification 15 (Well-formedness of selection encryptions in pre-encrypted ballots): TODO: E.G. 2.0 - implement"));

        // Verification 16 (Adherence to vote limits in pre-encrypted ballots)
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "Verification 16 (Adherence to vote limits in pre-encrypted ballots): TODO: E.G. 2.0 - implement"));

        // Verification 17 (Validation of confirmation codes in pre-encrypted ballots)
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "Verification 17 (Validation of confirmation codes in pre-encrypted ballots): TODO: E.G. 2.0 - implement"));

        // Verification 18 (Validation of short codes in pre-encrypted ballots)
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "Verification 18 (Validation of short codes in pre-encrypted ballots): TODO: E.G. 2.0 - implement"));

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
        // TODO: Issue #433 - E.G. 2.0 - implement
        // https://github.com/microsoft/electionguard-core2/issues/433
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
        // TODO: Issue #433 - E.G. 2.0 - implement
        // https://github.com/microsoft/electionguard-core2/issues/433
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 1.F: TODO: E.G. 2.0 - implement"));

        // Verification 1.G
        // TODO: Issue #433 - E.G. 2.0 - implement
        // https://github.com/microsoft/electionguard-core2/issues/433
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 1.G: TODO: E.G. 2.0 - implement"));

        // Verification 1.H
        // TODO: Issue #433 - E.G. 2.0 - implement
        // https://github.com/microsoft/electionguard-core2/issues/433
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
    /// (2.C) The challenge ci,j is correctly computed as 
    ///       ci,j = H (HP ; 0x10, i, j, Ki,j , hi,j ).
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
                // TODO: Issue #433 - E.G. 2.0 - implement
                // https://github.com/microsoft/electionguard-core2/issues/433
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

        return Task.FromResult(new VerificationResult("Verification 3 (Election public-key validation)", results));
    }

    /// <summary>
    /// Verification 4 (Extended base hash validation)
    /// An election verifier must verify the correct computation of the extended base hash. 
    /// (4.A) HE = H(HB;0x12,K).
    /// </summary>
    private static Task<VerificationResult> VerifyExtendedBaseHash(ElectionConstants constants, CiphertextElectionContext context)
    {
        var results = new List<VerificationResult>();

        // Verification 4.A
        // TODO: Issue #433 - E.G. 2.0 - implement
        // https://github.com/microsoft/electionguard-core2/issues/433
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 4.A: TODO: E.G. 2.0 - implement"));

        return Task.FromResult(new VerificationResult("Verification 4 (Extended base hash validation)", results));
    }

    /// <summary>
    /// Verification 5 (Well-formedness of selection encryptions)
    /// For each selectable option on each cast ballot, an election verifier must compute the values
    /// (5.1) aj = g^vj ·α^cj mod p for all 0 ≤ j ≤ R,
    /// (5.2) bj = K^wj ·β^cj mod p,where wj =(vj −jcj) mod q for all 0 ≤ j ≤ R, 
    /// (5.3) c = H(HE;0x21,K,α,β,a0,b0,a1,b1,...,aR,bR),
    ///       where R is the option selection limit. An election verifier must then confirm the following:
    /// (5.A) The given values α and β are in the set Zrp.
    ///       (A value x is in Zrp if and only if x is an integer such that 0 ≤ x < p and xq mod p = 1.)
    /// (5.B) The given values cj each satisfy 0 ≤ cj <2256 for all 0 ≤ j ≤ R.
    /// (5.C) The given values vj are each in the set Zq for all 0 ≤ j ≤ R.
    ///       (A value x is in Zq if and only if x is an integer such that 0 ≤ x < q.)
    /// (5.D) The equation c = (c0 + c1 + ··· + cR) mod q is satisfied.
    /// </sumary>
    public static async Task<VerificationResult> VerifySelectionEncryptions(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> ballots
    )
    {
        var results = new List<VerificationResult>();

        foreach (var ballot in ballots)
        {
            var contestResults = new List<VerificationResult>();
            foreach (var contest in ballot.Contests)
            {
                var selectionResults = new List<VerificationResult>();
                foreach (var selection in contest.Selections)
                {
                    // spec version 1.x uses disjunctive proofs, 
                    // which are implemented in this version
                    // note they are numbered Verification 4 in spec v1.5.3
                    // even though they are in section 5 in spec v2.0.0
                    selectionResults.Add(await VerifyDisjunctiveProof(constants, context, selection));
                }
                contestResults.Add(new VerificationResult($"  - Verification 5: Contest {contest.ObjectId}", selectionResults));
            }
            results.Add(new VerificationResult($"- Verification 5: Ballot {ballot.ObjectId}", contestResults));
        }

        return new VerificationResult("Verification 5 (Correctness of selection encryptions)", results);
    }

    /// <summary>
    /// Verification 4 (Correctness of selection encryptions)
    /// For each possible selection on each cast ballot, an election verifier must compute the values
    /// (4.1) a0 = gv0 · αc0 mod p,
    /// (4.2) a1 = gv1 · αc1 mod p,
    /// (4.3) b0 = Kv0 · βc0 mod p,
    /// (4.4) b1 = Kw1 · βc1 mod p,wherew1 = (v1−c1) mod q, 
    /// (4.5) c = H(04,Q;K,α,β,a0,b0,a1,b1).
    /// An election verifier must then confirm the following:
    /// (4.A) The given values α and β are in the set Zrp. 
    ///       (A value x is in Zrp if and only if x is an integer such that 0 ≤ x < p and xq mod p = 1 is satisfied.)
    /// (4.B) The given values c0, c1, v0, and v1 are each in the set Zq. 
    ///       (A value x is in Zq if and only if x is an integer such that 0 ≤ x < q.)
    /// (4.C) The equation c = (c0 + c1) mod q is satisfied.
    /// </summary>
    private static Task<VerificationResult> VerifyDisjunctiveProof(
        ElectionConstants constants,
        CiphertextElectionContext context,
        CiphertextBallotSelection selection
    )
    {
        var results = new List<VerificationResult>();

        // Verification 4.1 - 𝑎0 = 𝑔^𝑣0 mod 𝑝 ⋅ 𝛼^𝑐0 mod 𝑝
        using var gv0 = BigMath.PowModP(constants.G, selection.Proof.ZeroResponse);
        using var ac0 = BigMath.PowModP(selection.Ciphertext.Pad, selection.Proof.ZeroChallenge);
        using var gv0ac0 = BigMath.MultModP(gv0, ac0);
        var consistent_gv0 = selection.Proof.ZeroPad.Equals(gv0ac0);
        results.Add(new VerificationResult(consistent_gv0, $"      - Verification 4.1: 𝑎0 = 𝑔^𝑣0 mod 𝑝 ⋅ 𝛼^𝑐0 mod 𝑝"));

        // Verification 4.2 - 𝑎1 = 𝑔^𝑣1 mod 𝑝 ⋅ 𝛼^𝑐1 mod 𝑝
        using var gv1 = BigMath.PowModP(constants.G, selection.Proof.OneResponse);
        using var ac1 = BigMath.PowModP(selection.Ciphertext.Pad, selection.Proof.OneChallenge);
        using var gv1ac1 = BigMath.MultModP(gv1, ac1);
        var consistent_gv1 = selection.Proof.OnePad.Equals(gv1ac1);
        results.Add(new VerificationResult(consistent_gv1, $"      - Verification 4.2: 𝑎1 = 𝑔^𝑣1 mod 𝑝 ⋅ 𝛼^𝑐1 mod 𝑝"));

        // Verification 4.3 - 𝑏0 = 𝐾^𝑣0 mod 𝑝 ⋅ 𝛽^𝑐0 mod 𝑝
        using var kv0 = BigMath.PowModP(context.ElGamalPublicKey, selection.Proof.ZeroResponse);
        using var bc0 = BigMath.PowModP(selection.Ciphertext.Data, selection.Proof.ZeroChallenge);
        using var kv0bc0 = BigMath.MultModP(kv0, bc0);
        var consistent_kv0 = selection.Proof.ZeroData.Equals(kv0bc0);
        results.Add(new VerificationResult(consistent_kv0, $"      - Verification 4.3: 𝑏0 = 𝐾^𝑣0 mod 𝑝 ⋅ 𝛽^𝑐0 mod 𝑝"));

        // Verification 4.4 - 𝑏1 = 𝐾^w1 mod 𝑝 ⋅ 𝛽^𝑐1 mod 𝑝
        using var w1 = BigMath.SubModQ(selection.Proof.OneResponse, selection.Proof.OneChallenge);
        using var kw1 = BigMath.PowModP(context.ElGamalPublicKey, w1);
        using var bc1 = BigMath.PowModP(selection.Ciphertext.Data, selection.Proof.OneChallenge);
        using var kw1bc1 = BigMath.MultModP(kw1, bc1);
        var consistent_kw1 = selection.Proof.OneData.Equals(kw1bc1);
        results.Add(new VerificationResult(consistent_kw1, $"      - Verification 4.4: 𝑏1 = 𝐾^w1 mod 𝑝 ⋅ 𝛽^𝑐1 mod 𝑝"));

        // Verification 4.5 - c = H(04,Q,K,α,β,a0,b0,a1,b1)
        // using var recomputedHash = Hash.HashElems(
        //     Hash.Prefix_SelectionProof,
        //     context.CryptoExtendedBaseHash,
        //     context.ElGamalPublicKey,
        //     selection.Ciphertext.Pad,
        //     selection.Ciphertext.Data,
        //     selection.Proof.ZeroPad,
        //     selection.Proof.ZeroData,
        //     selection.Proof.OnePad,
        //     selection.Proof.OneData);
        // var consistent_hash = selection.Proof.Challenge.Equals(recomputedHash);
        // results.Add(new VerificationResult(consistent_hash, $"      - Verification 4.5: c = H(04,Q,K,α,β,a0,b0,a1,b1)"));

        // TODO: Issue #433 - E.G. 2.0 - implement
        // https://github.com/microsoft/electionguard-core2/issues/433
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 4.5: TODO: E.G. 2.0 - implement"));


        // Verification 4.A
        var inBoundsAlpha = selection.Ciphertext.Pad.IsInBounds();
        results.Add(new VerificationResult(inBoundsAlpha, $"      - Verification 4.A: Alpha is in bounds"));
        var inboundsBeta = selection.Ciphertext.Data.IsInBounds();
        results.Add(new VerificationResult(inboundsBeta, $"      - Verification 4.A: Beta is in bounds"));

        // Verification 4.B
        var inBoundsC0 = selection.Proof.ZeroChallenge.IsInBounds();
        results.Add(new VerificationResult(inBoundsC0, $"      - Verification 4.B: C0 is in bounds"));
        var inBoundsC1 = selection.Proof.OneChallenge.IsInBounds();
        results.Add(new VerificationResult(inBoundsC1, $"      - Verification 4.B: C1 is in bounds"));
        var inBoundsV0 = selection.Proof.ZeroResponse.IsInBounds();
        results.Add(new VerificationResult(inBoundsV0, $"      - Verification 4.B: V0 is in bounds"));
        var inBoundsV1 = selection.Proof.OneResponse.IsInBounds();
        results.Add(new VerificationResult(inBoundsV1, $"      - Verification 4.B: V1 is in bounds"));

        // Verification 4.C
        using var c0c1 = BigMath.AddModQ(selection.Proof.ZeroChallenge, selection.Proof.OneChallenge);
        var consistent_c = c0c1.Equals(selection.Proof.Challenge);
        results.Add(new VerificationResult(consistent_c, $"      - Verification 4.C: C = (c0 + c1) mod q"));

        // note numbered Verification 4 in spec v1.5.3
        // note numbered Verification 5 in spec v2.0.0
        return Task.FromResult(new VerificationResult($"    - Verification 4: Selection {selection.ObjectId}", results));
    }

    // spec version 2.0 uses range proofs, which are not implemented in this version
    private static Task<VerificationResult> VerifySelectionRangeProof(
        ElectionConstants constants,
        CiphertextElectionContext context,
        CiphertextBallotSelection selection
    )
    {
        // TODO: Issue #420 - Implement range proofs for selection
        // https://github.com/microsoft/electionguard-core2/issues/420

        var results = new List<VerificationResult>();

        // Verification 5.1
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 5.1: TODO: E.G. 2.0 - implement"));

        // Verification 5.2
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 5.2: TODO: E.G. 2.0 - implement"));

        // Verification 5.3
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 5.3: TODO: E.G. 2.0 - implement"));

        // Verification 5.A
        var inBoundsAlpha = selection.Ciphertext.Pad.IsInBounds();
        results.Add(new VerificationResult(inBoundsAlpha, $"- Verification 5.A: Alpha is in bounds"));
        var inboundsBeta = selection.Ciphertext.Data.IsInBounds();
        results.Add(new VerificationResult(inboundsBeta, $"- Verification 5.A: Beta is in bounds"));

        // note numbered Verification 4 in spec v1.5.3
        // note numbered Verification 5 in spec v2.0.0
        return Task.FromResult(new VerificationResult($"    - Verification 5: Selection {selection.ObjectId}", results));
    }

    /// <summary>
    /// Verification 6 (Adherence to vote limits)
    /// For each contest on each cast ballot, an election verifier must compute the contest totals 
    /// (6.1) α ̄ = ∏iαi mod p,
    /// (6.2) β ̄ = ∏iβi mod p,
    /// where the (αi,βi) represent all possible selections for the contest, as well as the values
    /// (6.3) aj = g^vj ·α ̄^cj mod p for all 0 ≤ j ≤ L,
    /// (6.4) bj = K^wj · β ̄^cj mod p, 
    ///       where wj = (vj −jcj) mod q for all 0 ≤ j ≤ L, 
    /// (6.5) c = H(HE;0x21,K,α ̄,β ̄,a0,b0,a1,b1,...,aL,bL),
    ///       where L is the contest selection limit. 
    /// An election verifier must then confirm the following:
    /// (6.A) The given values αi and βi are each in Zrp.
    /// (6.B) The given values cj each satisfy 0 ≤ cj < 2^256.
    /// (6.C) The given values vj are each in Zq for all 0 ≤ j ≤ L. 
    /// (6.D) The equation c = (c0 + c1 + ··· + cL) mod q is satisfied.
    /// </summary>
    public static async Task<VerificationResult> VerifyVoteLimits(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> ballots
    )
    {
        var results = new List<VerificationResult>();
        foreach (var ballot in ballots)
        {
            var contestResults = new List<VerificationResult>();
            foreach (var contest in ballot.Contests)
            {
                var accumulation = new ElGamalCiphertext(Constants.ONE_MOD_P, Constants.ONE_MOD_P);
                foreach (var selection in contest.Selections)
                {
                    accumulation = accumulation.Add(selection.Ciphertext);
                }
                // Verification 6.1
                var consistentA = accumulation.Pad.Equals(contest.CiphertextAccumulation.Pad);
                contestResults.Add(new VerificationResult(consistentA, $"- Verification 6.1: {contest.ObjectId} Consistent A"));

                // Verification 6.2
                var consistentB = accumulation.Data.Equals(contest.CiphertextAccumulation.Data);
                contestResults.Add(new VerificationResult(consistentB, $"- Verification 6.2: {contest.ObjectId} Consistent B"));

                // Verification 6.3 - 6.D
                contestResults.Add(await VerifyContestRangeProof(constants, context, manifest, contest));

                accumulation.Dispose();
            }
            results.Add(new VerificationResult($"    - Verification 6: Ballot {ballot.ObjectId}", contestResults));
        }

        return new VerificationResult("Verification 6 (Adherence to vote limits)", results);
    }

    private static Task<VerificationResult> VerifyContestRangeProof(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        CiphertextBallotContest contest
    )
    {
        var results = new List<VerificationResult>();

        // TODO: Issue #420 - implement range proofs for selection
        // https://github.com/microsoft/electionguard-core2/issues/420
        // 
        // Once the ZeroKnowledgeProof cpp struct is exposed across the api surface
        // reimplement this method by verifying all of the math is correct manually
        // instead of calling RangedChaumPedersenProof.IsValid. 

        // Verification 6.3 - aj = g^vj ·α ̄^cj mod p
        // Verification 6.4 - bj = K^wj · β ̄^cj mod p
        var temp_internal_check_is_valid = contest.Proof.IsValid(
            contest.CiphertextAccumulation,
            context.ElGamalPublicKey,
            context.CryptoExtendedBaseHash, Hash.Prefix_ContestProof);
        results.Add(new VerificationResult(temp_internal_check_is_valid.IsValid, $"      - Verification 6.3, 6.4, 6.A, 6.B, 6.C: temp internal check is valid"));

        // Verification 6.5
        // cannot verify the hash values because the hash values do not match the E.G. 2.0 Spec
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 6.5: TODO: E.G. 2.0 - implement"));

        // Verification 6.A
        // included in temp_internal_check_is_valid

        // Verification 6.B
        // included in temp_internal_check_is_valid

        // Verification 6.C
        // included in temp_internal_check_is_valid

        // Verification 6.D
        // cannot verify the hash values because the hash values do not match the E.G. 2.0 Spec
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 6.D: TODO: E.G. 2.0 - implement"));


        return Task.FromResult(new VerificationResult($"    - Verification 6: Contest {contest.ObjectId}", results));
    }

    /// <summary>
    /// Verification 7 (Validation of confirmation codes)
    /// An election verifier must confirm the following for each ballot B.
    /// (7.A) The contest hash χl for the contest with contest index l for all 1 ≤ l ≤ mB has been correctly
    ///       computed from the contest selection encryptions (αi,βi) as
    ///       χl = H(HE;0x23,l,K,α1,β1,α2,β2 ...,αm,βm).
    /// (7.B) The ballot confirmation code H(B) has been correctly computed from the contest hashes and if specified in the election manifest file from the additional byte array Baux as
    ///       H(B) = H(HE;0x24,χ1,χ2,...,χmB ,Baux).
    /// An election verifier must also verify the following.
    /// (7.C) There are no duplicate confirmation codes, 
    ///       i.e. among the set of submitted (cast and challenged) ballots, 
    ///       no two have the same confirmation code.
    /// Additionally, if the election manifest file specifies a hash chain, 
    /// an election verifier must confirm the following for each voting device.
    /// (7.D) The initial hash code H0 satisfies H0 = H(HE;0x24,Baux,0) and Baux,0 contains the unique voting device information.
    /// (7.E) For all 1 ≤ j ≤ l, the additional input byte array used to compute Hj = H(Bj) is equal to
    ///       Baux,j = H(Bj−1) ∥ Baux,0.
    /// (7.F) The final additional input byte array is equal to Baux = H(Bl ) ∥ Baux,0 ∥ b(“CLOSE”, 5) and
    ///       H(Bl) is the final confirmation code on this device.
    /// (7.G) The closing hash is correctly computed as H = H(HE;0x24,Baux).
    /// </summary>
    public static async Task<VerificationResult> VerifyConfirmationCodes(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> ballots
    )
    {
        var results = new List<VerificationResult>();

        foreach (var ballot in ballots)
        {
            results.Add(await VerifyConfirmationCodes_SpecV1x(constants, context, manifest, ballot));
        }

        // Verification 7.C
        var noDuplicates = ballots.Distinct(new CiphertextBallot.BallotCodeComparer()).Count() == ballots.Count;
        results.Add(new VerificationResult(noDuplicates, "- Verification 7.C: No duplicate confirmation codes"));

        // Verification 7.D
        // cannot verify the hash values because the hash values do not match the E.G. 2.0 Spec
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 7.D: TODO: E.G. 2.0 - implement"));

        // Verification 7.E
        // TODO: Issue #422
        // https://github.com/microsoft/electionguard-core2/issues/422
        // cannot verify the hash values because precompute is used, which breaks hash chaining
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 7.E: TODO: E.G. 2.0 - implement"));

        // Verification 7.F
        // TODO: Issue #422
        // https://github.com/microsoft/electionguard-core2/issues/422
        // cannot verify the hash values because precompute is used, which breaks hash chaining
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 7.F: TODO: E.G. 2.0 - implement"));

        // Verification 7.G
        // TODO: Issue #422
        // https://github.com/microsoft/electionguard-core2/issues/422
        // cannot verify the hash values because precompute is used, which breaks hash chaining
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 7.G: TODO: E.G. 2.0 - implement"));

        return new VerificationResult("Verification 7 (Validation of confirmation codes)", results);
    }

    private static Task<VerificationResult> VerifyConfirmationCodes_SpecV1x(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        CiphertextBallot ballot
    )
    {
        var results = new List<VerificationResult>();

        // Verification 6.A
        // TODO: Issue #433 - E.G. 2.0 - implement
        // https://github.com/microsoft/electionguard-core2/issues/433
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 6.A: TODO: E.G. 2.0 - implement"));

        // note numbered Verification 6 in spec v1.5.3
        // note numbered Verification 7 in spec v2.0.0
        return Task.FromResult(new VerificationResult($"Verification 6 Ballot: {ballot.ObjectId}", results));
    }

    private static Task<VerificationResult> VerifyConfirmationCodes_SpecV2x(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        CiphertextBallot ballot
    )
    {
        var results = new List<VerificationResult>();

        // Verification 7.A
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 7.A: TODO: E.G. 2.0 - implement"));

        // Verification 7.B
        results.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 7.B: TODO: E.G. 2.0 - implement"));

        return Task.FromResult(new VerificationResult($"Verification 7 Ballot: {ballot.ObjectId}", results));
    }

    /// <summary>
    /// Verification 8 (Correctness of ballot aggregation)
    /// An election verifier must confirm for each option in each contest in the election manifest that the aggregate encryption (A, B) satisfies
    /// (8.A) A = (∏jαj) mod p, 
    /// (8.B) B = (∏jβj) mod p,
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

        var internalManifest = new InternalManifest(manifest);
        var recomputedTally = new CiphertextTally(tally.Name, context, internalManifest);
        _ = recomputedTally.Accumulate(ballots);

        foreach (var (contestId, contest) in tally.Contests)
        {
            var contestResults = new List<VerificationResult>();
            foreach (var (selectionId, selection) in contest.Selections)
            {
                var recomputed = recomputedTally.Contests[contestId].Selections[selectionId].Ciphertext;

                // Verification 8.A
                var consistentA = selection.Ciphertext.Pad.Equals(recomputed.Pad);
                contestResults.Add(new VerificationResult(consistentA, $"- Verification 8.A: {selection.ObjectId} Consistent A"));

                // Verification 8.B
                var consistentB = selection.Ciphertext.Data.Equals(recomputed.Data);
                contestResults.Add(new VerificationResult(consistentB, $"- Verification 8.B: {selection.ObjectId} Consistent B"));
            }
            results.Add(new VerificationResult($"- Verification 8: Contest {contestId}", contestResults));
        }

        return Task.FromResult(new VerificationResult("Verification 8 (Correctness of ballot aggregation)", results));
    }

    /// <summary>
    /// Verification 9 (Correctness of decryptions)
    /// For each option in each contest on each tally, an election verifier must compute the values
    /// (9.1) M = B · T^−1 mod p, 
    /// (9.2) a = g^v ·K^c mod p, 
    /// (9.3) b = A^v ·M^c mod p.
    /// An election verifier must then confirm the following:
    /// (9.A) The given value v is in the set Zq.
    /// (9.B) The challenge value c satisfies c = H(HE;0x30,K,A,B,a,b,M).
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

        foreach (var (contestId, contest) in decryptedTally.Contests)
        {
            var contestResults = new List<VerificationResult>();
            foreach (var (selectionId, plaintext) in contest.Selections)
            {
                var encrypted = encryptedTally.Contests[contestId].Selections[selectionId];
                var ciphertext = encrypted.Ciphertext;

                // Verification 9.1 - M = B · T^−1 mod p
                using var m = BigMath.DivModP(ciphertext.Data, plaintext.Value);

                // since the proof object is nullable, we check that it is there before proceeding
                if (plaintext.Proof == null)
                {
                    contestResults.Add(new VerificationResult(false, $"- Verification 9.2: Selection {plaintext.ObjectId} Proof is null"));
                    contestResults.Add(new VerificationResult(false, $"- Verification 9.3: Selection {plaintext.ObjectId} Proof is null"));
                    contestResults.Add(new VerificationResult(false, $"- Verification 9.A: Selection {plaintext.ObjectId} Consistent Response V"));
                    continue;
                }

                // Verification 9.2 - 𝑎 = 𝑔^𝑣 • 𝐾^𝑐 mod 𝑝
                using var gv = BigMath.PowModP(constants.G, plaintext.Proof.Response);
                using var kc = BigMath.PowModP(context.ElGamalPublicKey, plaintext.Proof.Challenge);
                using var gvkc = BigMath.MultModP(gv, kc);
                var consistentA = plaintext.Proof.Pad.Equals(gvkc);
                contestResults.Add(new VerificationResult(consistentA, $"- Verification 9.2: Selection {plaintext.ObjectId} Consistent Commitment a"));

                // Verification 9.3 - 𝑏 = 𝐴^𝑣 • 𝑀^𝑐 mod 𝑝
                using var av = BigMath.PowModP(ciphertext.Pad, plaintext.Proof.Response);
                using var mc = BigMath.PowModP(m, plaintext.Proof.Challenge);
                using var avmc = BigMath.MultModP(av, mc);
                var consistentB = plaintext.Proof.Data.Equals(avmc);
                contestResults.Add(new VerificationResult(consistentB, $"- Verification 9.3: Selection {plaintext.ObjectId} Consistent Commitment b"));

                // Verification 9.A
                var consistentV = plaintext.Proof.Response.IsInBounds();
                contestResults.Add(new VerificationResult(consistentV, $"- Verification 9.A: Selection {plaintext.ObjectId} Consistent Response V"));

                // Verification 9.B
                var computedChallenge = Hash.HashElems(
                    Hash.Prefix_DecryptSelectionProof,
                    context.CryptoExtendedBaseHash,
                    context.ElGamalPublicKey,
                    ciphertext.Pad,
                    ciphertext.Data,
                    plaintext.Proof.Pad,
                    plaintext.Proof.Data,
                    m);

                var consistentC = plaintext.Proof.Challenge.Equals(computedChallenge);
                contestResults.Add(new VerificationResult(consistentC, $"- Verification 9.B: Selection {plaintext.ObjectId} Computed challenge does not match proof"));
            }
            results.Add(new VerificationResult($"- Verification 9: Contest {contestId}", contestResults));
        }

        return Task.FromResult(new VerificationResult("Verification 9 (Correctness of decryptions)", results));
    }

    /// <summary>
    /// Verification 10 (Validation of correct decryption of tallies)
    /// An election verifier must confirm the following equations for each option in each contest in the election manifest.
    /// (10.A) T = K^t mod p.
    /// An election verifier must also confirm that the text labels listed in the election record tallies 
    /// match the corresponding text labels in the election manifest. 
    /// For each contest in a decrypted tally, an election verifier must confirm the following.
    /// (10.B) The contest text label occurs as a contest label in the list of contests in the election manifest.
    /// (10.C) For each option in the contest, the option text label occurs as an option label for the contest
    /// in the election manifest.
    /// (10.D) For each option text label listed for this contest in the election manifest, the option label
    ///        occurs for a option in the decrypted tally contest.
    /// An election verifier must also confirm the following.
    /// (10.E) For each contest text label that occurs in at least one submitted ballot, 
    ///        that contest text label occurs in the list of contests in the corresponding tally.
    /// </summary>
    public static Task<VerificationResult> VerifyTallyDecryptionMetadata(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        CiphertextTallyRecord encryptedTally,
        PlaintextTally decryptedTally
    )
    {
        // convert to the internal manifest representation
        var internalManifest = new InternalManifest(manifest);

        var results = new List<VerificationResult>();
        foreach (var contest in internalManifest.Contests)
        {
            var plaintextContest = decryptedTally.Contests[contest.ObjectId];
            var encryptedContest = encryptedTally.Contests[contest.ObjectId];

            var contestResults = new List<VerificationResult>();
            var verificationCResults = new List<VerificationResult>();
            var verificationDResults = new List<VerificationResult>();
            foreach (var selection in contest.Selections)
            {
                var plaintext = plaintextContest.Selections[selection.ObjectId];
                var encrypted = encryptedContest.Selections[selection.ObjectId];

                // Verification 10.A - T = K^t mod p
                using var t = BigMath.PowModP(context.ElGamalPublicKey, plaintext.Tally);
                var consistentT = plaintext.Value.Equals(t);
                contestResults.Add(new VerificationResult(consistentT, $"- Verification 10.A: Selection {plaintext.ObjectId} Consistent Tally T"));

                // Verification 10.C
                // same as 10.B, but for the plaintext selection
                var plaintextOptionLabelIsValid = plaintext.DescriptionHash.Equals(selection.DescriptionHash);
                var plaintextOptionSequenceIsValid = plaintext.SequenceOrder == selection.SequenceOrder;
                verificationCResults.Add(new VerificationResult(plaintextOptionLabelIsValid && plaintextOptionSequenceIsValid, $"- Verification 10.C: Selection {plaintext.ObjectId} in manifest"));

                // Verification 10.D
                // same as 10.B, but for the encrypted selection
                var encryptedOptionLabelIsValid = encrypted.DescriptionHash.Equals(selection.DescriptionHash);
                var encryptedOptionSequenceIsValid = encrypted.SequenceOrder == selection.SequenceOrder;
                verificationDResults.Add(new VerificationResult(encryptedOptionLabelIsValid && encryptedOptionSequenceIsValid, $"- Verification 10.D: Selection {encrypted.ObjectId} in manifest"));
            }

            // Verification 10.B
            // we do not propagate the friendly labels into the plaintext tally selection
            // so instead we check tht the hash of the plaintext selection matches the hash of the manifest selection
            // and that the sequence order also matches. We have implicitly checked the objectId by this point via the dictionary lookup
            var plaintextContestHashIsValid = plaintextContest.DescriptionHash.Equals(contest.DescriptionHash);
            var plaintextContestSequenceIsValid = plaintextContest.SequenceOrder == contest.SequenceOrder;
            contestResults.Add(new VerificationResult(plaintextContestHashIsValid && plaintextContestSequenceIsValid, $"- Verification 10.B: Contest {plaintextContest.ObjectId} in manifest"));

            // Verification 10.C - add the results
            contestResults.AddRange(verificationCResults);

            // Verification 10.D - add the results
            contestResults.AddRange(verificationDResults);

            // Verification 10.E
            // same as 10.B, but for the encrypted contest
            var encryptedContestHashIsValid = encryptedContest.DescriptionHash.Equals(contest.DescriptionHash);
            var encryptedContestSequenceIsValid = encryptedContest.SequenceOrder == contest.SequenceOrder;
            contestResults.Add(new VerificationResult(encryptedContestHashIsValid && encryptedContestSequenceIsValid, $"- Verification 10.E: Contest {encryptedContest.ObjectId} in manifest"));

            results.Add(new VerificationResult($"- Verification 10: Contest {contest.ObjectId}", contestResults));
        }

        return Task.FromResult(new VerificationResult("Verification 10 (Validation of correct decryption of tallies)", results));
    }

    /// <summary>
    /// Verification 12 (Correctness of decryptions for challenged ballots)
    /// For each challenged ballot, for each option in each contest on 
    /// the challenged ballot, an election verifier must compute the values
    /// (12.1) M = β · S−1 mod p, 
    /// (12.2) a = g^v ·K^c mod p, 
    /// (12.3) b = α^v ·M^c mod p.
    /// An election verifier must then confirm the following.
    /// (12.A) The given value v is in the set Zq.
    /// (12.B) The challenge value c satisfies 
    ///        c = H(HE;0x30,K,α,β,a,b,M).
    /// </summary>
    public static Task<VerificationResult> VerifyBallotDecryptions(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> encryptedBallots,
        List<PlaintextTallyBallot> decryptedBallots
    )
    {
        var results = new List<VerificationResult>();

        foreach (var ballot in encryptedBallots)
        {
            var ballotResults = new List<VerificationResult>();
            var plaintext = decryptedBallots.FirstOrDefault(x => x.BallotId == ballot.ObjectId);
            foreach (var contest in ballot.Contests)
            {
                var contestResults = new List<VerificationResult>();
                var plaintextContest = plaintext!.Contests[contest.ObjectId];
                foreach (var selection in contest.Selections)
                {
                    var plaintextSelection = plaintextContest.Selections[selection.ObjectId];

                    // Verification 12 (challenge ballots) is similar to Verification 9 (tally)

                    // Verification 12.1 - M = β · S−1 mod p,
                    using var m = BigMath.DivModP(selection.Ciphertext.Data, plaintextSelection.Value);

                    // since the proof object is nullable, we check that it is there before proceeding
                    if (plaintextSelection.Proof == null)
                    {
                        contestResults.Add(new VerificationResult(false, $"- Verification 12.2: Selection {plaintextSelection.ObjectId} Proof is null"));
                        contestResults.Add(new VerificationResult(false, $"- Verification 12.3: Selection {plaintextSelection.ObjectId} Proof is null"));
                        contestResults.Add(new VerificationResult(false, $"- Verification 12.A: Selection {plaintextSelection.ObjectId} Consistent Response V"));
                        continue;
                    }

                    // Verification 12.2 - 𝑎 = 𝑔^𝑣 • 𝐾^𝑐 mod 𝑝
                    using var gv = BigMath.PowModP(constants.G, plaintextSelection.Proof!.Response);
                    using var kc = BigMath.PowModP(context.ElGamalPublicKey, plaintextSelection.Proof.Challenge);
                    using var gvkc = BigMath.MultModP(gv, kc);
                    var consistentA = plaintextSelection.Proof!.Pad.Equals(gvkc);
                    contestResults.Add(new VerificationResult(consistentA, $"- Verification 12.2: Selection {plaintextSelection.ObjectId} Consistent Commitment a"));

                    // Verification 12.3 - 𝑏 = 𝐴^𝑣 • 𝑀^𝑐 mod 𝑝
                    using var av = BigMath.PowModP(selection.Ciphertext.Pad, plaintextSelection.Proof.Response);
                    using var mc = BigMath.PowModP(m, plaintextSelection.Proof.Challenge);
                    using var avmc = BigMath.MultModP(av, mc);
                    var consistentB = plaintextSelection.Proof!.Data.Equals(avmc);
                    contestResults.Add(new VerificationResult(consistentB, $"- Verification 12.3: Selection {plaintextSelection.ObjectId} Consistent Commitment b"));

                    // Verification 12.A
                    var consistentV = plaintextSelection.Proof.Response.IsInBounds();
                    contestResults.Add(new VerificationResult(consistentV, $"- Verification 12.A: Selection {plaintextSelection.ObjectId} Consistent Response V"));

                    // Verification 12.B
                    // cannot verify the hash values because the hash values do not match the E.G. 2.0 Spec
                    contestResults.Add(new VerificationResult(IsValidwithKnownSpecDeviations, "- Verification 12.B: TODO: E.G. 2.0 - implement"));
                }
                ballotResults.Add(new VerificationResult($"- Verification 12: Contest {plaintextContest.ObjectId}", contestResults));
            }
            results.Add(new VerificationResult($"- Verification 12: Ballot {ballot.ObjectId}", ballotResults));
        }

        return Task.FromResult(new VerificationResult("Verification 12 (Correctness of decryptions for challenged ballots)", results));
    }

    /// <summary>
    /// Verification 13 (Validation of correct decryption of challenged ballots)
    /// An election verifier must confirm the correct decryption of each selection σ in each contest. 
    /// (13.A) S = K^σ mod p.
    /// An election verifier must also confirm that the challenged ballot is well-formed, 
    /// i.e. for each contest on the challenged ballot, it must confirm the following.
    /// (13.B) For each option in the contest, the selection σ is a valid value — usually either a 0 or a 1. 
    /// (13.C) The sum of all selections in the contest is at most the selection limit L for that contest.
    /// An election verifier must also confirm that for each decrypted challenged ballot, 
    /// the selections listed in text match the corresponding text in the election manifest.
    /// (13.D) The contest text label occurs as a contest label in the list of contests in the election manifest. 
    /// (13.E) For each option in the contest, the option text label occurs as an option label for the contest
    /// in the election manifest.
    /// (13.F) For each option text label listed for this contest in the election manifest, the option label
    /// occurs for a option in the decrypted challenged ballot.
    /// </summary>
    public static Task<VerificationResult> VerifyBallotDecryptionMetadata(
        ElectionConstants constants,
        CiphertextElectionContext context,
        Manifest manifest,
        List<CiphertextBallot> encryptedBallots,
        List<PlaintextTallyBallot> decryptedBallots
    )
    {
        // convert to the internal manifest representation
        var internalManifest = new InternalManifest(manifest);

        var results = new List<VerificationResult>();
        foreach (var ballot in decryptedBallots)
        {
            var encryptedBallot = encryptedBallots.FirstOrDefault(x => x.ObjectId == ballot.BallotId)!;

            var ballotResults = new List<VerificationResult>();
            foreach (var (_, contest) in ballot.Contests)
            {
                // Verification 13 (challenge ballots) is similar to Verification 10 (tally)

                var manifestContest = internalManifest.Contests.FirstOrDefault(x => x.ObjectId == contest.ObjectId)!;
                var encryptedContest = encryptedBallot.Contests.FirstOrDefault(x => x.ObjectId == contest.ObjectId)!;

                var contestResults = new List<VerificationResult>();
                var verificationEResults = new List<VerificationResult>();
                var verificationFResults = new List<VerificationResult>();

                var selectionSum = 0UL;
                foreach (var (_, selection) in contest.Selections)
                {
                    var description = manifestContest.Selections.FirstOrDefault(x => x.ObjectId == selection.ObjectId)!;
                    var encrypted = encryptedContest.Selections.FirstOrDefault(x => x.ObjectId == selection.ObjectId)!;

                    // Verification 13.A - S = K^σ mod p
                    using var s = BigMath.PowModP(context.ElGamalPublicKey, selection.Tally);
                    var consistentT = selection.Value.Equals(s);
                    contestResults.Add(new VerificationResult(consistentT, $"- Verification 13.A: Selection {description.ObjectId} Consistent Decryption S"));

                    // Verification 13.B
                    var consistentSelection = selection.Tally is 0 or 1;
                    contestResults.Add(new VerificationResult(consistentSelection, $"- Verification 13.B: Selection {description.ObjectId} Consistent Selection"));

                    // Verification 13.C
                    selectionSum += selection.Tally;

                    // Verification 13.E
                    // same as 13.D, but for the plaintext selection
                    var plaintextOptionLabelIsValid = description.DescriptionHash.Equals(selection.DescriptionHash);
                    var plaintextOptionSequenceIsValid = description.SequenceOrder == selection.SequenceOrder;
                    verificationEResults.Add(new VerificationResult(plaintextOptionLabelIsValid && plaintextOptionSequenceIsValid, $"- Verification 13.E: Selection {description.ObjectId} in manifest"));

                    // Verification 13.F
                    // same as 13.D, but for the encrypted selection
                    var encryptedOptionLabelIsValid = encrypted.DescriptionHash.Equals(selection.DescriptionHash);
                    var encryptedOptionSequenceIsValid = encrypted.SequenceOrder == selection.SequenceOrder;
                    verificationFResults.Add(new VerificationResult(encryptedOptionLabelIsValid && encryptedOptionSequenceIsValid, $"- Verification 13.F: Selection {encrypted.ObjectId} in manifest"));
                }

                // Verification 13.C
                var consistentSelectionSum = selectionSum <= manifestContest.VotesAllowed;
                contestResults.Add(new VerificationResult(consistentSelectionSum, $"- Verification 13.C: Contest {manifestContest.ObjectId} Consistent Selection Sum"));

                // Verification 13.D
                // we do not propagate the friendly labels into the plaintext tally selection
                // so instead we check tht the hash of the plaintext selection matches the hash of the manifest selection
                // and that the sequence order also matches. We have implicitly checked the objectId by this point via the dictionary lookup
                var plaintextContestHashIsValid = manifestContest.DescriptionHash.Equals(contest.DescriptionHash);
                var plaintextContestSequenceIsValid = manifestContest.SequenceOrder == contest.SequenceOrder;
                contestResults.Add(new VerificationResult(plaintextContestHashIsValid && plaintextContestSequenceIsValid, $"- Verification 13.D: Contest {contest.ObjectId} in manifest"));

                // Verification 10.E
                // same as 13.D, but for the encrypted contest
                var encryptedContestHashIsValid = encryptedContest.DescriptionHash.Equals(contest.DescriptionHash);
                var encryptedContestSequenceIsValid = encryptedContest.SequenceOrder == contest.SequenceOrder;
                contestResults.Add(new VerificationResult(encryptedContestHashIsValid && encryptedContestSequenceIsValid, $"- Verification 13.D: Contest {encryptedContest.ObjectId} in manifest"));

                // Verification 10.E - add the results
                contestResults.AddRange(verificationEResults);

                // Verification 10.F - add the results
                contestResults.AddRange(verificationFResults);

                ballotResults.Add(new VerificationResult($"- Verification 13: Contest {contest.ObjectId}", contestResults));
            }
            results.Add(new VerificationResult($"- Verification 13: Ballot {ballot.BallotId}", ballotResults));
        }

        return Task.FromResult(new VerificationResult("Verification 13 (Validation of correct decryption of tallies)", results));
    }
}
