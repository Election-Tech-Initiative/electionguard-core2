namespace ElectionGuard
{
    /// <summary>
    /// Metadata for encryption
    ///
    /// The encrypt object is used for encrypting ballots.
    ///
    /// </summary>
    public class Encrypt
    {
        /// <summary>
        /// Encrypt a specific `BallotSelection` in the context of a specific `BallotContest`
        /// </summary>
        /// <param name="plaintext">the selection in the valid input form</param>
        /// <param name="description">the `SelectionDescription` from the `ContestDescription`
        ///                           which defines this selection's structure</param>
        /// <param name="elgamalPublicKey">the public key (K) used to encrypt the ballot</param>
        /// <param name="cryptoExtendedBaseHash">the extended base hash of the election</param>
        /// <param name="nonceSeed">an `ElementModQ` used as a header to seed the `Nonce` generated
        ///                          for this selection. this value can be (or derived from) the
        ///                          Contest nonce, but no relationship is required</param>
        /// <param name="shouldVerifyProofs">specify if precomputed values should be used</param>
        /// <param name="usePrecomputedValues">specify if the proofs should be verified using precomputed values (default False)</param>
        /// <returns>A `CiphertextBallotSelection`</returns>
        public static CiphertextBallotSelection Selection(
            PlaintextBallotSelection plaintext,
            SelectionDescription description,
            ElementModP elgamalPublicKey,
            ElementModQ cryptoExtendedBaseHash,
            ElementModQ nonceSeed,
            bool shouldVerifyProofs = true,
            bool usePrecomputedValues = false
        )
        {
            var status = NativeInterface.Encrypt.Selection(
                    plaintext.Handle, description.Handle, elgamalPublicKey.Handle,
                    cryptoExtendedBaseHash.Handle, nonceSeed.Handle, shouldVerifyProofs,
                    usePrecomputedValues,
                    out var ciphertext);
            status.ThrowIfError();
            return ciphertext.IsInvalid ? null : new CiphertextBallotSelection(ciphertext);
        }

        /// <summary>
        /// Encrypt a specific `BallotContest` in the context of a specific `Ballot`
        ///
        /// This method accepts a contest representation that only includes `True` selections.
        /// It will fill missing selections for a contest with `False` values, and generate `placeholder`
        /// selections to represent the number of seats available for a given contest.  By adding `placeholder`
        /// votes
        /// </summary>
        /// <param name="plaintext">the selection in the valid input form</param>
        /// <param name="description">the `ContestDescriptionWithPlaceholders` from the `ContestDescription`
        ///                           which defines this contest's structure</param>
        /// <param name="elgamalPublicKey">the public key (K) used to encrypt the ballot</param>
        /// <param name="cryptoExtendedBaseHash">the extended base hash of the election</param>
        /// <param name="nonceSeed">an `ElementModQ` used as a header to seed the `Nonce` generated
        ///                          for this contest. this value can be (or derived from) the
        ///                          Ballot nonce, but no relationship is required</param>
        /// <param name="shouldVerifyProofs">specify if the proofs should be verified prior to returning (default True)</param>
        /// <param name="usePrecomputedValues">specify if the precompute values should be used</param>
        /// <returns>A `CiphertextBallotContest`</returns>
        public static CiphertextBallotContest Contest(
            PlaintextBallotContest plaintext,
            ContestDescription description,
            ElementModP elgamalPublicKey,
            ElementModQ cryptoExtendedBaseHash,
            ElementModQ nonceSeed,
            bool shouldVerifyProofs = true,
            bool usePrecomputedValues = false
        )
        {
            var status = NativeInterface.Encrypt.Contest(
                plaintext.Handle,
                description.Handle,
                elgamalPublicKey.Handle,
                cryptoExtendedBaseHash.Handle,
                nonceSeed.Handle,
                shouldVerifyProofs,
                usePrecomputedValues,
                out var ciphertext);

            status.ThrowIfError();
            return ciphertext.IsInvalid ? null : new CiphertextBallotContest(ciphertext);
        }

        /// <summary>
        /// Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`
        ///
        /// This method accepts a ballot representation that only includes `True` selections.
        /// It will fill missing selections for a contest with `False` values, and generate `placeholder`
        /// selections to represent the number of seats available for a given contest.  By adding `placeholder`
        /// votes
        ///
        /// This method also allows for ballots to exclude passing contests for which the voter made no selections.
        /// It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
        /// This function can also take advantage of PrecomputeBuffers to speed up the encryption process.
        /// when using precomputed values, the application looks in the `PrecomputeBufferContext` for values
        /// and uses them for the encryptions. You must preload the `PrecomputeBufferContext` prior to calling this function
        /// with `shouldUsePrecomputedValues` set to `true`, otherwise the function will fall back to realtime generation.
        ///
        /// Because PrecomputeBuffers require a random nonce, calling this function with `shouldUsePrecomputedValues`
        /// set to `true` while also providing a nonce will result in an error.
        /// </summary>
        /// <param name="ballot">the selection in the valid input form</param>
        /// <param name="internalManifest">the `InternalManifest` which defines this ballot's structure</param>
        /// <param name="context">all the cryptographic context for the election</param>
        /// <param name="ballotCodeSeed">Hash from previous ballot or hash from device</param>
        /// <param name="nonce">an optional value used to seed the `Nonce` generated for this ballot
        ///                     if this value is not provided, the secret generating mechanism of the OS provides its own</param>
        /// <param name="timestamp">timestamp to use</param>
        /// <param name="shouldVerifyProofs">specify if the proofs should be verified prior to returning (default True)</param>
        /// <param name="usePrecomputedValues">specify if precomputed values should be used (default True)</param>
        /// <returns>A `CiphertextBallot`</returns>
        public static CiphertextBallot Ballot(
            PlaintextBallot ballot,
            InternalManifest internalManifest,
            CiphertextElectionContext context,
            ElementModQ ballotCodeSeed,
            ElementModQ nonce = null,
            ulong timestamp = 0,
            bool shouldVerifyProofs = true,
            bool usePrecomputedValues = false)
        {
            if (nonce == null)
            {
                var status = NativeInterface.Encrypt.Ballot(
                    ballot.Handle,
                    internalManifest.Handle,
                    context.Handle,
                    ballotCodeSeed.Handle,
                    shouldVerifyProofs,
                    usePrecomputedValues,
                    out var ciphertext);
                status.ThrowIfError();
                return ciphertext.IsInvalid ? null : new CiphertextBallot(ciphertext);
            }
            else
            {
                var status = NativeInterface.Encrypt.Ballot(
                    ballot.Handle,
                    internalManifest.Handle,
                    context.Handle,
                    ballotCodeSeed.Handle,
                    nonce.Handle,
                    timestamp,
                    shouldVerifyProofs,
                    out var ciphertext);
                status.ThrowIfError();
                return ciphertext.IsInvalid ? null : new CiphertextBallot(ciphertext);
            }
        }

        /// <summary>
        /// Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`
        ///
        /// This method accepts a ballot representation that only includes `True` selections.
        /// It will fill missing selections for a contest with `False` values, and generate `placeholder`
        /// selections to represent the number of seats available for a given contest.  By adding `placeholder`
        /// votes
        ///
        /// This method also allows for ballots to exclude passing contests for which the voter made no selections.
        /// It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
        ///
        /// This version of the encrypt method returns a `compact` version of the ballot that includes a minimal representation
        /// of the plaintext ballot along with the crypto parameters that are required to expand the ballot
        /// </summary>
        /// <param name="ballot">the selection in the valid input form</param>
        /// <param name="internalManifest">the `InternalManifest` which defines this ballot's structure</param>
        /// <param name="context">all the cryptographic context for the election</param>
        /// <param name="ballotCodeSeed">Hash from previous ballot or hash from device</param>
        /// <param name="nonce">an optional value used to seed the `Nonce` generated for this ballot
        ///                     if this value is not provided, the secret generating mechanism of the OS provides its own</param>
        /// <param name="timestamp">timestamp to use</param>
        /// <param name="shouldVerifyProofs">specify if the proofs should be verified prior to returning (default True)</param>
        /// <returns>A `CiphertextBallot`</returns>
        public static CompactCiphertextBallot CompactBallot(
            PlaintextBallot ballot,
            InternalManifest internalManifest,
            CiphertextElectionContext context,
            ElementModQ ballotCodeSeed,
            ElementModQ nonce = null,
            ulong timestamp = 0,
            bool shouldVerifyProofs = true)
        {
            if (nonce == null)
            {
                var status = NativeInterface.Encrypt.CompactBallot(
                    ballot.Handle, internalManifest.Handle, context.Handle,
                    ballotCodeSeed.Handle, shouldVerifyProofs,
                    out var ciphertext);
                status.ThrowIfError();
                return ciphertext.IsInvalid ? null : new CompactCiphertextBallot(ciphertext);
            }
            else
            {
                var status = NativeInterface.Encrypt.CompactBallot(
                    ballot.Handle, internalManifest.Handle, context.Handle,
                    ballotCodeSeed.Handle, nonce.Handle, timestamp, shouldVerifyProofs,
                    out var ciphertext);
                status.ThrowIfError();
                return ciphertext.IsInvalid ? null : new CompactCiphertextBallot(ciphertext);
            }
        }
    }
}
