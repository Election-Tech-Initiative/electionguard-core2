/// @file ciphertext_ballot.generated.h
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

#ifndef CiphertextBallot

/**
 * @brief The unique ballot id that is meaningful to the consuming application.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_object_id A pointer to the output ObjectId.  The caller is responsible for freeing it.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_get_object_id(
	eg_ciphertext_ballot_t *handle,
	char **out_object_id
	);

/**
 * @brief The Object Id of the ballot style in the election manifest.  This value is used to determine which contests to expect on the ballot, to fill in missing values, and to validate that the ballot is well-formed
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_style_id A pointer to the output StyleId.  The caller is responsible for freeing it.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_get_style_id(
	eg_ciphertext_ballot_t *handle,
	char **out_style_id
	);

/**
 * @brief Hash of the complete Election Manifest to which this ballot belongs
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_manifest_hash A pointer to the output ManifestHash.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_get_manifest_hash(
	eg_ciphertext_ballot_t *handle,
	eg_element_mod_q_t **out_manifest_hash
	);

/**
 * @brief The seed hash for the ballot.  It may be the encryption device hash, the hash of a previous ballot or the hash of some other value that is meaningful to the consuming application.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_ballot_code_seed A pointer to the output BallotCodeSeed.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_get_ballot_code_seed(
	eg_ciphertext_ballot_t *handle,
	eg_element_mod_q_t **out_ballot_code_seed
	);

/**
 * @brief Get the size of the contests collection
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @return The value of the property
 */
EG_API uint64_t eg_ciphertext_ballot_get_contests_size(
	eg_ciphertext_ballot_t *handle
	);

/**
 * @brief The unique ballot code for this ballot that is derived from the ballot seed, the timestamp, and the hash of the encrypted values
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_ballot_code A pointer to the output BallotCode.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_get_ballot_code(
	eg_ciphertext_ballot_t *handle,
	eg_element_mod_q_t **out_ballot_code
	);

/**
 * @brief The timestamp indicating when the ballot was encrypted as measured by the encryption device.  This value does not provide units as it is up to the host system to indicate the scale. Typically a host may use seconds since epoch or ticks since epoch.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @return The value of the property
 */
EG_API uint64_t eg_ciphertext_ballot_get_timestamp(
	eg_ciphertext_ballot_t *handle
	);

/**
 * @brief The nonce value used to encrypt all values in the ballot
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_nonce A pointer to the output Nonce.  The value is a reference and is not owned by the caller.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_get_nonce(
	eg_ciphertext_ballot_t *handle,
	eg_element_mod_q_t **out_nonce
	);

/**
 * Get a contest at a specific index
 * @param[in] in_index The index of the contest
 * @param[out] out_get_contest_at_index_ref An opaque pointer to the CiphertextBallotContest
 *                               The value is a reference and is not owned by the caller
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_get_contest_at_index(
	eg_ciphertext_ballot_t *handle,
	uint64_t in_index,
	eg_ciphertext_ballot_contest_t **out_get_contest_at_index_ref
	);

/**
 * Given an encrypted Ballot, validates the encryption state against a specific ballot seed and public key by verifying the states of this ballot's members (BallotContest's and BallotSelection's). Calling this function expects that the object is in a well-formed encrypted state with the `contests` populated with valid encrypted ballot selections, and the ElementModQ `manifest_hash` also populated. Specifically, the seed in this context is the hash of the Election Manifest, or whatever `ElementModQ` was used to populate the `manifest_hash` field.
 */
EG_API bool eg_ciphertext_ballot_is_valid_encryption(
	eg_ciphertext_ballot_t *handle,
	eg_element_mod_q_t *in_manifest_hash,
	eg_element_mod_p_t *in_el_gamal_public_key,
	eg_element_mod_q_t *in_crypto_extended_base_hash
	);

/**
 * Frees the memory held by the CiphertextBallot
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_free(eg_ciphertext_ballot_t *handle);

#endif // ifndef CiphertextBallot

#ifdef __cplusplus
}
#endif
