namespace ElectionGuard.Encryption.Ballot
{
    /// <summary>
    /// A selection on a ballot or in a tally
    /// </summary>
    public interface IElectionSelection
    {
        /// <summary>
        /// The object id
        /// </summary>
        string ObjectId { get; }

        /// <summary>
        /// The sequence order
        /// </summary>
        ulong SequenceOrder { get; }

        /// <summary>
        /// The description hash
        /// </summary>
        ElementModQ DescriptionHash { get; }
    }

    /// <summary>
    /// A selection on a ballot or in a tally
    /// </summary>
    public interface ICiphertextSelection : IElectionSelection
    {
        /// <summary>
        /// The ciphertext
        /// </summary>
        ElGamalCiphertext Ciphertext { get; }
    }
}
