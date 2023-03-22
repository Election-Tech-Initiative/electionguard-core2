using ElectionGuard.Decryption.Concurrency;
using ElectionGuard.Encryption;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Tally
{
    /// <summary>
    /// A CiphertextTallySelection is a homomorphic accumulation of ElGamalCiphertexts that represent the
    /// encrypted votes for a particular selection in a contest.
    /// </summary>
    public class CiphertextTallySelection : DisposableBase, ICiphertextSelection
    {
        /// <summary>
        /// The object id of the selection
        /// </summary>
        public string ObjectId { get; init; } = default!;

        /// <summary>
        /// The sequence order of the selection
        /// </summary>
        public ulong SequenceOrder { get; init; }

        /// <summary>
        /// The hash of the SelectionDescription
        /// </summary>
        public ElementModQ DescriptionHash { get; init; } = default!;

        /// <summary>
        /// The encrypted representation of the sum of all ballots for the selection
        /// </summary>
        public ElGamalCiphertext Ciphertext { get; private set; } = default!;

        private readonly AsyncLock _lock = new();

        public CiphertextTallySelection(
            string objectId, ulong sequenceOrder, ElementModQ descriptionHash)
        {
            ObjectId = objectId;
            SequenceOrder = sequenceOrder;
            DescriptionHash = descriptionHash;
            Ciphertext = new ElGamalCiphertext(Constants.ONE_MOD_P, Constants.ONE_MOD_P);
        }

        public CiphertextTallySelection(SelectionDescription selectionDescription)
        {
            ObjectId = selectionDescription.ObjectId;
            SequenceOrder = selectionDescription.SequenceOrder;
            DescriptionHash = selectionDescription.CryptoHash();
            Ciphertext = new ElGamalCiphertext(Constants.ONE_MOD_P, Constants.ONE_MOD_P);
        }

        public CiphertextTallySelection(
            SelectionDescription selectionDescription, ElGamalCiphertext ciphertext)
        {
            ObjectId = selectionDescription.ObjectId;
            SequenceOrder = selectionDescription.SequenceOrder;
            DescriptionHash = selectionDescription.CryptoHash();
            Ciphertext = ciphertext;
        }

        /// <summary>
        /// Homomorphically add the specified value to the message
        /// </summary>
        public ElGamalCiphertext Accumulate(CiphertextBallotSelection selection)
        {
            return selection.ObjectId != ObjectId || selection.DescriptionHash != DescriptionHash
                ? throw new ArgumentException("Selection does not match")
                : Accumulate(selection.Ciphertext);
        }

        public async Task<ElGamalCiphertext> AccumulateAsync(
            CiphertextBallotSelection selection,
            CancellationToken cancellationToken = default)
        {
            using (await _lock.LockAsync(cancellationToken))
            {
                return Accumulate(selection);
            }
        }

        /// <summary>
        /// Homomorphically add the specified values to the message
        /// </summary>
        public ElGamalCiphertext Accumulate(List<CiphertextBallotSelection> selections)
        {
            return selections.Any(
                i => i.ObjectId != ObjectId || i.DescriptionHash != DescriptionHash)
                    ? throw new ArgumentException("Selection does not match")
                    : Accumulate(selections.Select(i => i.Ciphertext));
        }

        public async Task<ElGamalCiphertext> AccumulateAsync(
            List<CiphertextBallotSelection> selections,
            CancellationToken cancellationToken = default)
        {
            using (await _lock.LockAsync(cancellationToken))
            {
                return Accumulate(selections);
            }
        }

        /// <summary>
        /// Homomorphically add the specified value to the message
        /// </summary>
        public ElGamalCiphertext Accumulate(ElGamalCiphertext ciphertext)
        {
            return Accumulate(new[] { ciphertext });
        }

        public async Task<ElGamalCiphertext> AccumulateAsync(
            ElGamalCiphertext ciphertext,
            CancellationToken cancellationToken = default)
        {
            using (await _lock.LockAsync(cancellationToken))
            {
                return Accumulate(ciphertext);
            }
        }

        /// <summary>
        /// Homomorphically add the specified values to the message
        /// </summary>
        public ElGamalCiphertext Accumulate(
            IEnumerable<ElGamalCiphertext> ciphertexts)
        {
            var newValue = ElGamal.Add(ciphertexts.Append(Ciphertext));
            Ciphertext.Dispose();
            Ciphertext = newValue;
            return Ciphertext;
        }

        public async Task<ElGamalCiphertext> AccumulateAsync(
            IEnumerable<ElGamalCiphertext> ciphertexts,
            CancellationToken cancellationToken = default)
        {
            using (await _lock.LockAsync(cancellationToken))
            {
                return Accumulate(ciphertexts);
            }
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            DescriptionHash.Dispose();
            Ciphertext.Dispose();
        }
    }
}
