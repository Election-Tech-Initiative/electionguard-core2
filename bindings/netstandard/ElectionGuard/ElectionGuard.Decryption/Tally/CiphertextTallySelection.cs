using ElectionGuard.ElectionSetup.Concurrency;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Tally
{
    /// <summary>
    /// A CiphertextTallySelection is a homomorphic accumulation of ElGamalCiphertexts that represent the
    /// encrypted votes for a particular selection in a contest.
    /// </summary>
    public class CiphertextTallySelection : DisposableBase,
        ICiphertextSelection, IEquatable<CiphertextTallySelection>
    {
        /// <summary>
        /// The object id of the selection
        /// </summary>
        public string ObjectId { get; init; }

        /// <summary>
        /// The sequence order of the selection
        /// </summary>
        public ulong SequenceOrder { get; init; }

        /// <summary>
        /// The hash of the SelectionDescription
        /// </summary>
        public ElementModQ DescriptionHash { get; init; }

        /// <summary>
        /// The encrypted representation of the sum of all ballots for the selection
        /// </summary>
        public ElGamalCiphertext Ciphertext { get; private set; }

        private readonly AsyncLock _mutex = new();
        private readonly object _lock = new();

        public CiphertextTallySelection(
            string objectId, ulong sequenceOrder, ElementModQ descriptionHash)
        {
            ObjectId = objectId;
            SequenceOrder = sequenceOrder;
            DescriptionHash = new(descriptionHash);
            Ciphertext = new ElGamalCiphertext(Constants.ONE_MOD_P, Constants.ONE_MOD_P);
        }

        public CiphertextTallySelection(SelectionDescription selection)
        {
            ObjectId = selection.ObjectId;
            SequenceOrder = selection.SequenceOrder;
            DescriptionHash = new(selection.CryptoHash());
            Ciphertext = new ElGamalCiphertext(Constants.ONE_MOD_P, Constants.ONE_MOD_P);
        }

        public CiphertextTallySelection(
            SelectionDescription selection, ElGamalCiphertext ciphertext)
        {
            ObjectId = selection.ObjectId;
            SequenceOrder = selection.SequenceOrder;
            DescriptionHash = new(selection.CryptoHash());
            Ciphertext = new(ciphertext);
        }

        public CiphertextTallySelection(CiphertextTallySelection other)
        {
            ObjectId = other.ObjectId;
            SequenceOrder = other.SequenceOrder;
            DescriptionHash = new(other.DescriptionHash);
            Ciphertext = new(other.Ciphertext);
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

        /// <summary>
        /// Homomorphically add the specified value to the message
        /// </summary>
        public async Task<ElGamalCiphertext> AccumulateAsync(
            CiphertextBallotSelection selection,
            CancellationToken cancellationToken = default)
        {
            using (await _mutex.LockAsync(cancellationToken))
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
            using (await _mutex.LockAsync(cancellationToken))
            {
                return Accumulate(selections);
            }
        }

        /// <summary>
        /// Homomorphically add the specified value to the message
        /// </summary>
        public ElGamalCiphertext Accumulate(ElGamalCiphertext ciphertext)
        {
            lock (_lock)
            {
                var newValue = Ciphertext.Add(ciphertext);
                var oldValue = Ciphertext;
                Ciphertext = newValue;
                oldValue.Dispose();
                return Ciphertext;
            }
        }

        public async Task<ElGamalCiphertext> AccumulateAsync(
            ElGamalCiphertext ciphertext,
            CancellationToken cancellationToken = default)
        {
            using (await _mutex.LockAsync(cancellationToken))
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
            lock (_lock)
            {
                var newValue = ElGamal.Add(ciphertexts.Append(Ciphertext));
                var oldValue = Ciphertext;
                Ciphertext = newValue;
                oldValue.Dispose();
                return Ciphertext;
            }
        }

        public async Task<ElGamalCiphertext> AccumulateAsync(
            IEnumerable<ElGamalCiphertext> ciphertexts,
            CancellationToken cancellationToken = default)
        {
            using (await _mutex.LockAsync(cancellationToken))
            {
                return Accumulate(ciphertexts);
            }
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            DescriptionHash.Dispose();
            Ciphertext.Dispose();
            _mutex.Dispose();
        }

        # region Equality

        public bool Equals(CiphertextTallySelection? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ObjectId == other.ObjectId &&
                   SequenceOrder == other.SequenceOrder &&
                   DescriptionHash.Equals(other.DescriptionHash) &&
                   Ciphertext.Equals(other.Ciphertext);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is CiphertextTallySelection other && Equals(other));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ObjectId, SequenceOrder, DescriptionHash, Ciphertext);
        }

        public static bool operator ==(CiphertextTallySelection? left, CiphertextTallySelection? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CiphertextTallySelection? left, CiphertextTallySelection? right)
        {
            return !Equals(left, right);
        }

        # endregion
    }
}
