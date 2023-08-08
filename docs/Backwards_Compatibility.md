# Backwards Compatibility

The core mathematical functions for Electionguard 2.0 differ significantly from Electionguard 1.0 and are not backwards compatible. Effort is made to maintain serialization compatibility for the Electionguard data structures however compatibility is not guaranteed for all functions.

## Key Differences

- Where possible Proofs have ben simplified.
- Hashing is more consistent.
- Guardians are no longer included in the election record.
- ElGamal Encryption now uses the public key as the base for all exponentiations whicvh simplifies the proofs
- CiphertextBallotContests now include a range proof instead of a constant integer proof for the proof of selection limit.
- Decryption is in verifiable and completed in a protocol


Please refer to the specification at electionguard.vote for an exhaustive list of changes.