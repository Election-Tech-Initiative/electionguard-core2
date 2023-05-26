using System.Collections.Generic;

namespace ElectionGuard.Ballot
{

    public interface ICiphertextContest : IElectionContest
    {
        /// <summary>
        /// The ciphertext
        /// </summary>
        IReadOnlyList<ICiphertextSelection> Selections { get; }
    }
}
