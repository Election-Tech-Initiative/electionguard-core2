using ElectionGuard.Decryption.Tally;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// Decryption methods for decrypting with known secret values.
/// </summary>
public static class DecryptWithSecretsExtensions
{
    // when decrypting with secrets, we do not currently have a proof
    [Obsolete("This method is obsolete and kept for testing purposes only when decrypting with secret values during unit testing. Use the overload with a proof.")]
    public static void Update(this PlaintextTallySelection self, ulong tally, ElementModP publicKey)
    {
        // recreate a decrypted value using the public key
        using var decrypted = BigMath.PowModP(publicKey, tally);
        self.Update(tally, decrypted, self.Proof);
    }

    /// <summary>
    /// Decrypts a <see cref="CiphertextTally" /> using the provided <see cref="ElementModQ" /> secret key.
    /// This method is primarily for testing purposes and should not be used in production.
    /// </summary>
    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        ElementModQ secretKey, ElementModP publicKey)
    {
        var plaintextTally = new PlaintextTally(
            self.TallyId, self.Name, self.Manifest);

        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextTally.Contests.First(
                x => x.Key == contest.Key).Value;
            foreach (var selection in contest.Value.Selections)
            {
                var ciphertext = selection.Value.Ciphertext;
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.Key).Value;

                var value = ciphertext.Decrypt(secretKey, publicKey);
                plaintextSelection.Update(value ?? 0, publicKey);
            }
        }
        return plaintextTally;
    }

    /// <summary>
    /// Decrypts a <see cref="CiphertextBallot" /> using the provided <see cref="ElementModQ" /> secret key.
    /// This method is primarily for testing purposes and should not be used in production.
    /// </summary>
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        InternalManifest manifest,
        ElementModQ secretKey, ElementModP publicKey)
    {
        var plaintextBallot = new PlaintextTallyBallot(
            self.ObjectId, self.ObjectId, self.ObjectId, self.StyleId, manifest);

        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextBallot.Contests.First(
                x => x.Key == contest.ObjectId).Value;
            foreach (var selection in contest.Selections
                .Where(x => x.IsPlaceholder == false))
            {
                var ciphertext = selection.Ciphertext;
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.ObjectId).Value;

                var value = ciphertext.Decrypt(secretKey, publicKey);
                plaintextSelection.Update(value ?? 0, publicKey);
            }
        }
        return plaintextBallot;
    }

    /// <summary>
    /// Decrypts a <see cref="CiphertextBallot" /> using the provided <see cref="ElementModQ" /> nonce value.
    /// This override can be used to decrypt a single ballot.
    /// </summary>
    public static PlaintextBallot Decrypt(
        this CiphertextBallot self,
        InternalManifest manifest,
        CiphertextElectionContext context,
        ElementModQ? nonceSeed = null,
        bool skipValidation = false,
        bool removePlaceholders = true)
    {
        if (skipValidation == false)
        {
            // check the enctyption validation
            var isValid = self.IsValidEncryption(
                manifest.ManifestHash, context.ElGamalPublicKey, context.CryptoExtendedBaseHash);
            if (isValid == false)
            {
                throw new Exception($"contest {self.ObjectId} is not valid");
            }
        }

        if (nonceSeed is null)
        {
            // if the nonce seed is not provided, then we need to calculate it from the ballot
            nonceSeed = self.Nonce != null
                ? CiphertextBallot.NonceSeed(manifest.ManifestHash, self.ObjectId, self.Nonce)
                : null;
        }
        else
        {
            // calculate the nonce using the provided seed
            nonceSeed = CiphertextBallot.NonceSeed(manifest.ManifestHash, self.ObjectId, nonceSeed);
        }

        if (nonceSeed is null)
        {
            throw new Exception($"nonce is null");
        }

        // iterate over the ballot contests and decrypt each one
        var plaintextContests = new List<PlaintextBallotContest>();
        foreach (var contest in self.Contests)
        {
            var description = manifest.Contests.First(
                x => x.ObjectId == contest.ObjectId);
            var plaintext = contest.Decrypt(
                description, context, nonceSeed, skipValidation, removePlaceholders);

            // only decrypt the actual selections on the ballot
            if (plaintext.Selections.Any(x => x.IsPlaceholder == false || removePlaceholders == false))
            {
                plaintextContests.Add(plaintext);
            }
        }

        return new PlaintextBallot(
            self.ObjectId, self.StyleId, plaintextContests.ToArray());
    }

    /// <summary>
    /// Decrypts a <see cref="CiphertextBallotContest" /> using the provided <see cref="ElementModQ" /> nonce value.
    /// This override can be used to decrypt a single ballot contest.
    /// </summary>
    public static PlaintextBallotContest Decrypt(
        this CiphertextBallotContest self,
        ContestDescriptionWithPlaceholders description,
        CiphertextElectionContext context,
        ElementModQ nonceSeed,
        bool skipValidation = false,
        bool removePlaceholders = true)
    {
        if (skipValidation == false)
        {
            // check the enctyption validation
            var isValid = self.IsValidEncryption(
                description.DescriptionHash, context.ElGamalPublicKey, context.CryptoExtendedBaseHash);
            if (isValid == false)
            {
                throw new Exception($"contest {self.ObjectId} is not valid");
            }
        }

        var contestNonce = CiphertextBallotContest.ContestNonce(
            context, description.SequenceOrder, nonceSeed);

        if (self.Nonce is not null && self.Nonce != contestNonce)
        {
            throw new Exception($"nonce mismatch");
        }

        // iterate over the ballot selections and decrypt each one
        var plaintextSelections = new List<PlaintextBallotSelection>();
        foreach (var selection in self.Selections.Where(x => x.IsPlaceholder == false))
        {
            var descriptionSelection = description.Selections.First(
                x => x.ObjectId == selection.ObjectId);
            var plaintext = selection.Decrypt(
                descriptionSelection,
                context.ElGamalPublicKey,
                context.CryptoExtendedBaseHash,
                contestNonce,
                skipValidation);
            if (plaintext.IsPlaceholder == false || removePlaceholders == false)
            {
                plaintextSelections.Add(plaintext);
            }
        }

        return new PlaintextBallotContest(
            self.ObjectId, plaintextSelections.ToArray());
    }

    /// <summary>
    /// Decrypts a <see cref="CiphertextBallotSelection" /> using the provided <see cref="ElementModQ" /> nonce value.
    /// This override can be used to decrypt a single ballot selection.
    /// </summary>
    public static PlaintextBallotSelection Decrypt(
        this CiphertextBallotSelection self,
        SelectionDescription description,
        ElementModP publicKey,
        ElementModQ extendedBaseHash,
        ElementModQ? nonceSeed = null,
        bool skipValidation = false)
    {

        if (skipValidation == false)
        {
            var isValid = self.IsValidEncryption(
                description.DescriptionHash, publicKey, extendedBaseHash);
            if (isValid == false)
            {
                throw new Exception($"selection {self.ObjectId} is not valid");
            }
        }

        // if no nonce seed is provided then use the nonce from the ballot
        // which is common in some cases, such as when using precomputed values
        ElementModQ? nonce = null;
        if (nonceSeed is null)
        {
            nonce = self.Nonce;
        }
        else
        {
            // calculate the nonce using the provided seed
            nonce = Hash.HashElems(nonceSeed, description.SequenceOrder);
        }

        if (nonce is null)
        {
            throw new Exception($"nonce is null");
        }

        var plaintext = self.Ciphertext.Decrypt(publicKey, nonce);

        return new PlaintextBallotSelection(
            self.ObjectId, plaintext ?? 0, self.IsPlaceholder);
    }
}
