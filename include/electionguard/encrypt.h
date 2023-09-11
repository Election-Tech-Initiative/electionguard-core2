/// @file encrypt.h
#ifndef __ELECTIONGUARD_CPP_ENCRYPT_H_INCLUDED__
#define __ELECTIONGUARD_CPP_ENCRYPT_H_INCLUDED__

#include "ballot.h"
#include "ballot_compact.h"
#include "election.h"
#include "export.h"
#include "manifest.h"
#include "status.h"

#include <stdbool.h>

#ifdef __cplusplus
extern "C" {
#endif

#ifndef EncryptionDevice

struct eg_encryption_device_s;

/**
* Metadata for encryption device
*
* The encryption device is a stateful container that represents abstract hardware
* authorized to participate in a specific election.
*/
typedef struct eg_encryption_device_s eg_encryption_device_t;

/**
* Metadata for encryption device
*
* The encryption device is a stateful container that represents abstract hardware
* authorized to participate in a specific election.
*
* @param[in] in_device_uuid a unique identifier tied to the device hardware
* @param[in] in_session_uuid a unique identifier tied to the runtime session
* @param[in] in_launch_code a unique identifer tied to the election
* @param[in] in_location an arbitrary string meaningful to the external system
*                        such as a friendly name, description, or some other value
* @param[out] out_handle a handle to an `eg_encryption_device_t`.  Caller is responsible for lifecycle.
*/
EG_API eg_electionguard_status_t eg_encryption_device_new(uint64_t in_device_uuid,
                                                          uint64_t in_session_uuid,
                                                          uint64_t in_launch_code,
                                                          const char *in_location,
                                                          eg_encryption_device_t **out_handle);

EG_API eg_electionguard_status_t eg_encryption_device_free(eg_encryption_device_t *handle);

/**
 * @brief Get the timestamp for the encrypion device
 * 
 * @param[out] out_timestamp The timestamp for the encryption device
 *                      The caller is responsible for freeing the object.
 */
EG_API eg_electionguard_status_t eg_encryption_device_get_timestamp(eg_encryption_device_t *handle,
                                                                    uint64_t *out_timestamp);

/**
 * @brief Get the uuid for the encrypion device
 * 
 * @param[out] out_uuid The uuid code for the encryption device
 *                      The caller is responsible for freeing the object.
 */
EG_API eg_electionguard_status_t eg_encryption_device_get_uuid(eg_encryption_device_t *handle,
                                                               uint64_t *out_uuid);

/**
 * @brief Get the session uuid for the encrypion device
 * 
 * @param[out] out_session_id The session uuid for the encryption device
 *                      The caller is responsible for freeing the object.
 */
EG_API eg_electionguard_status_t
eg_encryption_device_get_session_uuid(eg_encryption_device_t *handle, uint64_t *out_session_id);

/**
 * @brief Get the launch code for the encrypion device
 * 
 * @param[out] out_launch_code The launch code for the encryption device
 *                      The caller is responsible for freeing the object.
 */
EG_API eg_electionguard_status_t
eg_encryption_device_get_launch_code(eg_encryption_device_t *handle, uint64_t *out_launch_code);

/**
 * @brief Get the location for the encrypion device
 * 
 * @param[out] out_location The lcoation for the encryption device
 *                      The caller is responsible for freeing the object.
 */
EG_API eg_electionguard_status_t eg_encryption_device_get_location(eg_encryption_device_t *handle,
                                                                   char **out_location,
                                                                   uint64_t *out_size);

/**
 * @brief Get the hash for the encrypion device
 * 
 * @param[out] out_hash The hash code for the encryption device
 *                      The caller is responsible for freeing the object.
 */
EG_API eg_electionguard_status_t eg_encryption_device_get_hash(eg_encryption_device_t *handle,
                                                               eg_element_mod_q_t **out_hash);

/**
 * @brief Creates an EncryptionDevice object from a [RFC-8259](https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1) UTF-8 encoded JSON string
 * 
 * @param[in] in_data The UTF-8 Encoded JSON data string
 * @param[out] out_handle a handle to an @ref eg_encryption_device_t object.
 *                        The caller is responsible for freeing the object.
 * @return eg_electionguard_status_t indicating success or failure 
 */
EG_API eg_electionguard_status_t
eg_encryption_device_from_json(char *in_data, eg_encryption_device_t **out_handle);

/**
 * @brief Get the JSON representation of this encrption device
 * 
 * 
 * @param[out] out_data The json representation of the encryption device
 * @param[out] out_size The size of this JSON representation
 */
EG_API eg_electionguard_status_t eg_encryption_device_to_json(eg_encryption_device_t *handle,
                                                              char **out_data, uint64_t *out_size);
#endif

#ifndef EncryptionMediator

struct eg_encryption_mediator_s;

/**
* An object for caching election and encryption state.
*
* the encryption mediator composes ballots by querying the encryption device
* for a hash of its metadata and incremental timestamps/
*
* this is a convenience wrapper around the encrypt methods
* and may not be suitable for all use cases.
*/
typedef struct eg_encryption_mediator_s eg_encryption_mediator_t;

EG_API eg_electionguard_status_t eg_encryption_mediator_new(
  eg_internal_manifest_t *in_manifest, eg_ciphertext_election_context_t *in_context,
  eg_encryption_device_t *in_encryption_device, eg_encryption_mediator_t **out_handle);

EG_API eg_electionguard_status_t eg_encryption_mediator_free(eg_encryption_mediator_t *handle);

EG_API eg_electionguard_status_t eg_encryption_mediator_compact_encrypt_ballot(
  eg_encryption_mediator_t *handle, eg_plaintext_ballot_t *in_plaintext,
  eg_compact_ciphertext_ballot_t **out_ciphertext_handle);

EG_API eg_electionguard_status_t eg_encryption_mediator_compact_encrypt_ballot_verify_proofs(
  eg_encryption_mediator_t *handle, eg_plaintext_ballot_t *in_plaintext,
  eg_compact_ciphertext_ballot_t **out_ciphertext_handle);

/**
* Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`.
*
* This method accepts a ballot representation that only includes `True` selections.
* It will fill missing selections for a contest with `False` values, and generate `placeholder`
* selections to represent the number of seats available for a given contest.
*
* This method also allows for ballots to exclude passing contests for which the voter made no selections.
* It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
*
* This function can also take advantage of PrecomputeBuffers to speed up the encryption process.
* when using precomputed values, the application looks in the `PrecomputeBufferContext` for values
* and uses them for the encryptions. You must preload the `PrecomputeBufferContext` prior to calling this function
* with `shouldUsePrecomputedValues` set to `true`, otherwise the function will fall back to realtime generation.
*
* @param[in] in_plaintext The plaintext representation of the ballot
* @param[in] in_use_precomputed_values True if the mediator should use the precomputed values
* @param[out] out_ciphertext_handle a handle to a `CiphertextBallot` opaque data structure.  Caller is responsible for lifecycle.
**/
EG_API eg_electionguard_status_t eg_encryption_mediator_encrypt_ballot(
  eg_encryption_mediator_t *handle, eg_plaintext_ballot_t *in_plaintext,
  bool in_use_precomputed_values, eg_ciphertext_ballot_t **out_ciphertext_handle);

EG_API eg_electionguard_status_t eg_encryption_mediator_encrypt_ballot_verify_proofs(
  eg_encryption_mediator_t *handle, eg_plaintext_ballot_t *in_plaintext,
  eg_ciphertext_ballot_t **out_ciphertext_handle);

#endif

#ifndef Encryption Functions

/**
* Encrypt a specific `BallotSelection` in the context of a specific `BallotContest`
*
* @param[in] in_plaintext the selection in the valid input form
* @param[in] in_description the `eg_selection_description_t` from the `ContestDescription`
*                           which defines this selection's structure
* @param[in] in_public_key the `eg_element_mod_p_t` public key (K) used to encrypt the ballot
* @param[in] in_crypto_extended_base_hash the `eg_element_mod_q_t` extended base hash of the election
* @param[in] in_nonce_seed an `eg_element_mod_q_t` used as a header to seed the `Nonce` generated
*                          for this selection. this value can be (or derived from) the
*                          BallotContest nonce, but no relationship is required
* @param[in] in_is_placeholder specifies if this is a placeholder selection
* @param[in] in_should_verify_proofs specify if the proofs should be verified prior to returning (default True)
* @param[in] in_use_precomputed_values: specify if the ballot generation should use precomputed values
* @param[out] out_handle a handle to an `eg_ciphertext_ballot_selection_t`.  Caller is responsible for lifecycle.
* @return eg_electionguard_status_t indicating success or failure
* @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
* @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
*/
EG_API eg_electionguard_status_t eg_encrypt_selection(
  eg_plaintext_ballot_selection_t *in_plaintext, eg_selection_description_t *in_description,
  eg_ciphertext_election_context_t *in_context, eg_element_mod_q_t *in_nonce_seed,
  bool in_is_placeholder, bool in_should_verify_proofs, bool in_use_precomputed_values,
  eg_ciphertext_ballot_selection_t **out_handle);

/**
* Encrypt a specific `BallotContest` in the context of a specific `Ballot`.
*
* This method accepts a contest representation that only includes `True` selections.
* It will fill missing selections for a contest with `False` values, and generate `placeholder`
* selections to represent the number of seats available for a given contest.  By adding `placeholder`
* votes
*
* @param[in] in_plaintext: the contest in the valid input form
* @param[in] in_description the `ContestDescriptionWithPlaceholders`
*        from the `ContestDescription` which defines this contest's structure
* @param[in] in_public_key the public key (k) used to encrypt the ballot
* @param[in] in_crypto_extended_base_hash the extended base hash of the election
* @param[in] in_nonce_seed an `ElementModQ` used as a header to seed the `Nonce` generated for this contest.
*                this value can be (or derived from) the Ballot nonce, but no relationship is required
* @param[in] should_verify_proofs: specify if the proofs should be verified prior to returning (default True)
* @param[in] in_use_precomputed_values: specify if the ballot generation should use precomputed values
* @param[out] out_handle a handle to an `eg_ciphertext_ballot_contest_t`. Caller is responsible for lifecycle.
*/
EG_API eg_electionguard_status_t
eg_encrypt_contest(eg_plaintext_ballot_contest_t *in_plaintext,
                   eg_contest_description_with_placeholders_t *in_description,
                   eg_ciphertext_election_context_t *in_context, eg_element_mod_q_t *in_nonce_seed,
                   bool in_should_verify_proofs, bool in_use_precomputed_values,
                   eg_ciphertext_ballot_contest_t **out_handle);

/**
* Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`.
*
* This method accepts a ballot representation that only includes `True` selections.
* It will fill missing selections for a contest with `False` values, and generate `placeholder`
* selections to represent the number of seats available for a given contest.
*
* This method also allows for ballots to exclude passing contests for which the voter made no selections.
* It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
*
* This function can also take advantage of PrecomputeBuffers to speed up the encryption process.
* when using precomputed values, the application looks in the `PrecomputeBufferContext` for values
* and uses them for the encryptions. You must preload the `PrecomputeBufferContext` prior to calling this function
* with `shouldUsePrecomputedValues` set to `true`, otherwise the function will fall back to realtime generation.
* @param[in] in_plaintext: the ballot in the valid input form
* @param[in] in_manifest: the `InternalManifest` which defines this ballot's structure
* @param[in] in_context: all the cryptographic context for the election
* @param[in] in_ballot_code_seed: Hash from previous ballot or starting hash from device
* @param[in] in_should_verify_proofs: specify if the proofs should be verified prior to returning (default True)
* @param[in] in_use_precomputed_values: specify if the ballot generation should use precomputed values
* @param[out] out_handle a handle to an `eg_ciphertext_ballot_t`. Caller is responsible for lifecycle.
*/
EG_API eg_electionguard_status_t eg_encrypt_ballot(eg_plaintext_ballot_t *in_plaintext,
                                                   eg_internal_manifest_t *in_manifest,
                                                   eg_ciphertext_election_context_t *in_context,
                                                   eg_element_mod_q_t *in_ballot_code_seed,
                                                   bool in_should_verify_proofs,
                                                   bool in_use_precomputed_values,
                                                   eg_ciphertext_ballot_t **out_handle);

/**
* Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`.
*
* This method accepts a ballot representation that only includes `True` selections.
* It will fill missing selections for a contest with `False` values, and generate `placeholder`
* selections to represent the number of seats available for a given contest.
*
* This method also allows for ballots to exclude passing contests for which the voter made no selections.
* It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
*
* @param[in] in_plaintext: the ballot in the valid input form
* @param[in] in_manifest: the `InternalManifest` which defines this ballot's structure
* @param[in] in_context: all the cryptographic context for the election
* @param[in] in_ballot_code_seed: Hash from previous ballot or starting hash from device
* @param[in] in_nonce: an optional value used to seed the `Nonce` generated for this ballot
*                if this value is not provided, the secret generating mechanism of the OS provides its own
*                entropy source
* @param[in] timestamp: the timestamp of the ballot
* @param[in] in_should_verify_proofs: specify if the proofs should be verified prior to returning (default True)

* @param[out] out_handle a handle to an `eg_ciphertext_ballot_t`. Caller is responsible for lifecycle.
*/
EG_API eg_electionguard_status_t eg_encrypt_ballot_with_nonce(
  eg_plaintext_ballot_t *in_plaintext, eg_internal_manifest_t *in_manifest,
  eg_ciphertext_election_context_t *in_context, eg_element_mod_q_t *in_ballot_code_seed,
  eg_element_mod_q_t *in_nonce, uint64_t timestamp, bool in_should_verify_proofs,
  eg_ciphertext_ballot_t **out_handle);

/**
* Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`.
*
* This method accepts a ballot representation that only includes `True` selections.
* It will fill missing selections for a contest with `False` values, and generate `placeholder`
* selections to represent the number of seats available for a given contest.
*
* This method also allows for ballots to exclude passing contests for which the voter made no selections.
* It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
*
* This version of the encrypt method returns a `compact` version of the ballot that includes a minimal representation
* of the plaintext ballot along with the crypto parameters that are required to expand the ballot
*
* @param[in] in_plaintext: the ballot in the valid input form
* @param[in] in_manifest: the `InternalManifest` which defines this ballot's structure
* @param[in] in_context: all the cryptographic context for the election
* @param[in] in_ballot_code_seed: Hash from previous ballot or hash from device
* @param[in] in_should_verify_proofs: specify if the proofs should be verified prior to returning (default True)
* @param[out] out_handle a handle to an `eg_compact_ciphertext_ballot_t`. Caller is responsible for lifecycle.
*/
EG_API eg_electionguard_status_t eg_encrypt_compact_ballot(
  eg_plaintext_ballot_t *in_plaintext, eg_internal_manifest_t *in_manifest,
  eg_ciphertext_election_context_t *in_context, eg_element_mod_q_t *in_ballot_code_seed,
  bool in_should_verify_proofs, eg_compact_ciphertext_ballot_t **out_handle);

/**
* Encrypt a specific `Ballot` in the context of a specific `CiphertextElectionContext`.
*
* This method accepts a ballot representation that only includes `True` selections.
* It will fill missing selections for a contest with `False` values, and generate `placeholder`
* selections to represent the number of seats available for a given contest.
*
* This method also allows for ballots to exclude passing contests for which the voter made no selections.
* It will fill missing contests with `False` selections and generate `placeholder` selections that are marked `True`.
*
* This version of the encrypt method returns a `compact` version of the ballot that includes a minimal representation
* of the plaintext ballot along with the crypto parameters that are required to expand the ballot
*
* @param[in] in_plaintext: the ballot in the valid input form
* @param[in] in_manifest: the `InternalManifest` which defines this ballot's structure
* @param[in] in_context: all the cryptographic context for the election
* @param[in] in_ballot_code_seed: Hash from previous ballot or hash from device
* @param[in] in_nonce: an optional value used to seed the `Nonce` generated for this ballot
*                if this value is not provided, the secret generating mechanism of the OS provides its own
* @param[in] timestamp: the timestamp of the ballot
* @param[in] in_should_verify_proofs: specify if the proofs should be verified prior to returning (default True)
* @param[out] out_handle a handle to an `eg_compact_ciphertext_ballot_t`. Caller is responsible for lifecycle.
*/
EG_API eg_electionguard_status_t eg_encrypt_compact_ballot_with_nonce(
  eg_plaintext_ballot_t *in_plaintext, eg_internal_manifest_t *in_manifest,
  eg_ciphertext_election_context_t *in_context, eg_element_mod_q_t *in_ballot_code_seed,
  eg_element_mod_q_t *in_nonce, uint64_t timestamp, bool in_should_verify_proofs,
  eg_compact_ciphertext_ballot_t **out_handle);

#endif

#ifdef __cplusplus
}
#endif
#endif /* __ELECTIONGUARD_CPP_ENCRYPT_H_INCLUDED__ */
