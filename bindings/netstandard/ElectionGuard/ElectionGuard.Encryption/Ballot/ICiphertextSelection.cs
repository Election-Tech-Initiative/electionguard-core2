namespace ElectionGuard.Ballot
{

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
