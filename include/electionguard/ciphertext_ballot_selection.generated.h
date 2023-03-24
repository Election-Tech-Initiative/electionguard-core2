/// @file ciphertext_ballot_selection.generated.h
#pragma once

#include "chaum_pedersen.h"
#include "elgamal.h"
#include "export.h"
#include "group.h"
#include "status.h"
#include "ballot.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef CiphertextBallotSelection

/**
 * @brief Get the objectId of the selection, which is the unique id for the selection in a specific contest described in the election manifest.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_object_id A pointer to the output ObjectId.  The caller is responsible for freeing it.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_selection_get_object_id(
	eg_ciphertext_ballot_selection_t *handle,
	char **out_object_id
	);

/**
 * @brief Get the sequence order of the selection
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @return The value of the property
 */
EG_API uint64_t eg_ciphertext_ballot_selection_get_sequence_order(
	eg_ciphertext_ballot_selection_t *handle
	);

/**
 * @brief Determines if this is a placeholder selection
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @return The value of the property
 */
EG_API bool eg_ciphertext_ballot_selection_get_is_placeholder(
	eg_ciphertext_ballot_selection_t *handle
	);

/**
 * @brief The hash of the string representation of the Selection Description from the election manifest
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_description_hash A pointer to the output DescriptionHash.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_selection_get_description_hash(
	eg_ciphertext_ballot_selection_t *handle,
	eg_element_mod_q_t **out_description_hash
	);

/**
 * @brief The encrypted representation of the vote field
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_ciphertext A pointer to the output Ciphertext.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_selection_get_ciphertext(
	eg_ciphertext_ballot_selection_t *handle,
	eg_elgamal_ciphertext_t **out_ciphertext
	);

/**
 * @brief The hash of the encrypted values
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_crypto_hash A pointer to the output CryptoHash.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_selection_get_crypto_hash(
	eg_ciphertext_ballot_selection_t *handle,
	eg_element_mod_q_t **out_crypto_hash
	);

/**
 * @brief The nonce used to generate the encryption. Sensitive &amp; should be treated as a secret
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_nonce A pointer to the output Nonce.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_selection_get_nonce(
	eg_ciphertext_ballot_selection_t *handle,
	eg_element_mod_q_t **out_nonce
	);

/**
 * @brief The proof that demonstrates the selection is an encryption of 0 or 1, and was encrypted using the `nonce`
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_proof A pointer to the output Proof.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_selection_get_proof(
	eg_ciphertext_ballot_selection_t *handle,
	eg_disjunctive_chaum_pedersen_proof_t **out_proof
	);

/**
 * Given an encrypted BallotSelection, generates a hash, suitable for rolling up into a hash / tracking code for an entire ballot. Of note, this particular hash examines the `encryptionSeed` and `message`, but not the proof. This is deliberate, allowing for the possibility of ElectionGuard variants running on much more limited hardware, wherein the Disjunctive Chaum-Pedersen proofs might be computed later on. In most cases the encryption_seed should match the `description_hash`.
 * @param[in] in_encryption_seed In most cases the encryption_seed should match the `description_hash`
 * @param[out] out_crypto_hash_with_ref An opaque pointer to the ElementModQ
 *                               The caller is responsible for freeing it.
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_selection_crypto_hash_with(
	eg_ciphertext_ballot_selection_t *handle,
	eg_element_mod_q_t *in_encryption_seed,
	eg_element_mod_q_t **out_crypto_hash_with_ref
	);

/**
 * Given an encrypted BallotSelection, validates the encryption state against a specific seed hash and public key. Calling this function expects that the object is in a well-formed encrypted state with the elgamal encrypted `message` field populated along with the DisjunctiveChaumPedersenProof`proof` populated. the ElementModQ `description_hash` and the ElementModQ `crypto_hash` are also checked.
 * @param[in] in_encryption_seed The hash of the SelectionDescription, or whatever `ElementModQ` was used to populate the `description_hash` field.
 * @param[in] in_el_gamal_public_key The election public key.
 * @param[in] in_crypto_extended_base_hash The extended base hash of the election.
 */
EG_API bool eg_ciphertext_ballot_selection_is_valid_encryption(
	eg_ciphertext_ballot_selection_t *handle,
	eg_element_mod_q_t *in_encryption_seed,
	eg_element_mod_p_t *in_el_gamal_public_key,
	eg_element_mod_q_t *in_crypto_extended_base_hash
	);

/**
 * Frees the memory held by the CiphertextBallotSelection
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_selection_free(eg_ciphertext_ballot_selection_t *handle);

#endif // ifndef CiphertextBallotSelection

#ifdef __cplusplus
}
#endif
