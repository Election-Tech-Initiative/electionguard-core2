using System.Collections.Generic;

namespace ElectionGuard
{
    /// <summary>
    /// A CiphertextBallotContest represents the selections made by a voter for a specific ContestDescription
    ///
    /// CiphertextBallotContest can only be a complete representation of a contest dataset.  While
    /// PlaintextBallotContest supports a partial representation, a CiphertextBallotContest includes all data
    /// necessary for a verifier to verify the contest.  Specifically, it includes both explicit affirmative
    /// and negative selections of the contest, as well as the placeholder selections that satisfy
    /// the ConstantChaumPedersen proof.
    ///
    /// Similar to `CiphertextBallotSelection` the consuming application can choose to discard or keep both
    /// the `nonce` and the `proof` in some circumstances.  For deterministic nonce's derived from the
    /// seed nonce, both values can be regenerated.  If the `nonce` for this contest is completely random,
    /// then it is required in order to regenerate the proof.
    /// </summary>
    public partial class CiphertextBallotContest : DisposableBase
    {
        /// <summary>
        /// The collection of selections for the contest
        /// </summary>
        public IReadOnlyList<CiphertextBallotSelection> Selections =>
            new ElectionGuardEnumerator<CiphertextBallotSelection>(
                () => (int)SelectionsSize,
                (index) => GetSelectionAtIndex((ulong)index)
            );

        internal CiphertextBallotContest(External.CiphertextBallotContestHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Get a selection at a specific index.
        /// </summary>
        public CiphertextBallotSelection GetSelectionAtIndex(ulong index)
        {
            var status = NativeInterface.CiphertextBallotContest.GetSelectionAtIndex(
                Handle, index, out var value);
            status.ThrowIfError();
            return new CiphertextBallotSelection(value);
        }

        /// <summary>
        /// Given an encrypted BallotContest, generates a hash, suitable for rolling up
        /// into a hash / tracking code for an entire ballot. Of note, this particular hash examines
        /// the `encryptionSeed` and `message`, but not the proof.
        /// This is deliberate, allowing for the possibility of ElectionGuard variants running on
        /// much more limited hardware, wherein the Constant Chaum-Pedersen proofs might be computed
        /// later on.
        ///
        /// In most cases the encryption_seed should match the `description_hash`
        /// </summary>
        public ElementModQ CryptoHashWith(ElementModQ encryptionSeed)
        {
            var status = NativeInterface.CiphertextBallotContest.CryptoHashWith(
                Handle, encryptionSeed.Handle, out var value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// An aggregate nonce for the contest composed of the nonces of the selections.
        /// Used when constructing the proof of selection limit
        /// </summary>
        public ElementModQ AggregateNonce()
        {
            var status = NativeInterface.CiphertextBallotContest.AggregateNonce(
                Handle, out var value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// Add the individual ballot_selections `message` fields together, suitable for use
        /// when constructing the proof of selection limit.
        /// </summary>
        public ElGamalCiphertext ElGamalAccumulate()
        {
            var status = NativeInterface.CiphertextBallotContest.ElGamalAccumulate(
                Handle, out var value);
            status.ThrowIfError();
            return new ElGamalCiphertext(value);
        }

        /// <summary>
        /// Given an encrypted BallotContest, validates the encryption state against
        /// a specific encryption seed and public key
        /// by verifying the accumulated sum of selections match the proof.
        /// Calling this function expects that the object is in a well-formed encrypted state
        /// with the `ballot_selections` populated with valid encrypted ballot selections,
        /// the ElementModQ `description_hash`, the ElementModQ `crypto_hash`,
        /// and the ConstantChaumPedersenProof all populated.
        /// Specifically, the seed hash in this context is the hash of the ContestDescription,
        /// or whatever `ElementModQ` was used to populate the `description_hash` field.
        /// </summary>
        public bool IsValidEncryption(
            ElementModQ encryptionSeed,
            ElementModP elGamalPublicKey,
            ElementModQ cryptoExtendedBaseHash)
        {
            return NativeInterface.CiphertextBallotContest.IsValidEncryption(
                Handle, encryptionSeed.Handle, elGamalPublicKey.Handle,
                cryptoExtendedBaseHash.Handle);
        }
    }
}
