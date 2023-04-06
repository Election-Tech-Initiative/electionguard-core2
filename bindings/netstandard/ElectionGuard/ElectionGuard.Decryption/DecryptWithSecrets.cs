
using ElectionGuard.Decryption.Decryption;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption;

public static class DecryptWithSecretsExtensions
{
    /// <summary>
    /// Decrypts a <see cref="CiphertextTally" /> using the provided <see cref="ElementModQ" /> secret key
    /// </summary>
    public static PlaintextTally Decrypt(
        this CiphertextTally self,
        ElementModQ secretKey)
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

                var value = ciphertext.Decrypt(secretKey);
                plaintextSelection.Tally += value ?? 0;
            }
        }
        return plaintextTally;
    }

    // should not be uised in production
    public static PlaintextTallyBallot Decrypt(
        this CiphertextBallot self,
        InternalManifest manifest,
        ElementModQ secretKey)
    {
        var plaintextBallot = new PlaintextTallyBallot(
            self.ObjectId, self.ObjectId, self.StyleId, manifest);

        foreach (var contest in self.Contests)
        {
            var plaintextContest = plaintextBallot.Contests.First(
                x => x.Key == contest.ObjectId).Value;
            foreach (var selection in contest.Selections.Where(x => x.IsPlaceholder == false))
            {
                var ciphertext = selection.Ciphertext;
                var plaintextSelection = plaintextContest.Selections.First(
                    x => x.Key == selection.ObjectId).Value;

                var value = ciphertext.Decrypt(secretKey);
                plaintextSelection.Tally += value ?? 0;

            }
        }
        return plaintextBallot;
    }

    public static PlaintextBallot Decrypt(
        this CiphertextBallot self,
        InternalManifest manifest,
        CiphertextElectionContext context,
        ElementModQ? nonceSeed = null,
        bool skipValidation = false,
        bool removePlaceholders = true)
    {
        return self.Decrypt(
            manifest,
            context.ElGamalPublicKey,
            context.CryptoExtendedBaseHash,
            nonceSeed,
            skipValidation,
            removePlaceholders);
    }

    public static PlaintextBallot Decrypt(
        this CiphertextBallot self,
        InternalManifest manifest,
        ElementModP publicKey,
        ElementModQ extendedBaseHash,
        ElementModQ? nonceSeed = null,
        bool skipValidation = false,
        bool removePlaceholders = true)
    {
        if (skipValidation == false)
        {
            var isValid = self.IsValidEncryption(
                manifest.ManifestHash, publicKey, extendedBaseHash);
            if (isValid == false)
            {
                throw new Exception($"contest {self.ObjectId} is not valid");
            }
        }

        if (nonceSeed is null)
        {
            // TODO: this is different
            nonceSeed = self.Nonce != null
                ? CiphertextBallot.NonceSeed(manifest.ManifestHash, self.ObjectId, self.Nonce)
                : null;
        }
        else
        {
            nonceSeed = CiphertextBallot.NonceSeed(manifest.ManifestHash, self.ObjectId, nonceSeed);
        }

        if (nonceSeed is null)
        {
            throw new Exception($"nonce is null");
        }

        var plaintextContests = new List<PlaintextBallotContest>();
        foreach (var contest in self.Contests)
        {
            var description = manifest.Contests.First(
                x => x.ObjectId == contest.ObjectId);
            var plaintext = contest.Decrypt(
                description, publicKey, extendedBaseHash, nonceSeed, skipValidation, removePlaceholders);
            if (plaintext.Selections.Any(x => x.IsPlaceholder == false || removePlaceholders == false))
            {
                plaintextContests.Add(plaintext);
            }
        }

        return new PlaintextBallot(
            self.ObjectId, self.StyleId, plaintextContests.ToArray());
    }

    public static PlaintextBallotContest Decrypt(
        this CiphertextBallotContest self,
        ContestDescriptionWithPlaceholders description,
        ElementModP publicKey,
        ElementModQ extendedBaseHash,
        ElementModQ? nonceSeed = null,
        bool skipValidation = false,
        bool removePlaceholders = true)
    {
        if (skipValidation == false)
        {
            var isValid = self.IsValidEncryption(
                description.DescriptionHash, publicKey, extendedBaseHash);
            if (isValid == false)
            {
                throw new Exception($"contest {self.ObjectId} is not valid");
            }
        }

        if (nonceSeed is null)
        {
            nonceSeed = self.Nonce;
        }
        else
        {
            var sequence = new Nonces(description.DescriptionHash, nonceSeed);
            nonceSeed = sequence.Get(description.SequenceOrder);
        }

        if (nonceSeed is null)
        {
            throw new Exception($"nonce is null");
        }

        if (self.Nonce is not null && self.Nonce != nonceSeed)
        {
            throw new Exception($"nonce mismatch");
        }

        var plaintextSelections = new List<PlaintextBallotSelection>();
        foreach (var selection in self.Selections.Where(x => x.IsPlaceholder == false))
        {
            var descriptionSelection = description.Selections.First(
                x => x.ObjectId == selection.ObjectId);
            var plaintext = selection.Decrypt(
                descriptionSelection, publicKey, extendedBaseHash, nonceSeed, skipValidation);
            if (plaintext.IsPlaceholder == false || removePlaceholders == false)
            {
                plaintextSelections.Add(plaintext);
            }
        }

        return new PlaintextBallotContest(
            self.ObjectId, plaintextSelections.ToArray());
    }


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

        ElementModQ? nonce = null;
        if (nonceSeed is null)
        {
            nonce = self.Nonce;
        }
        else
        {
            var sequence = new Nonces(description.DescriptionHash, nonceSeed);
            nonce = sequence.Get(description.SequenceOrder);
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
