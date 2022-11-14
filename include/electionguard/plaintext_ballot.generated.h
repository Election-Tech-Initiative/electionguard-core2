/// @file plaintext_ballot.generated.h
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

#ifndef PlaintextBallot

/**
 * @brief A unique Ballot ID that is relevant to the external system and must be unique within the dataset of the election.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_object_id A pointer to the output ObjectId.  The caller is responsible for freeing it.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_plaintext_ballot_get_object_id(
	eg_plaintext_ballot_t *handle,
	char **out_object_id
	);

/**
 * @brief The Object Id of the ballot style in the election manifest.  This value is used to determine which contests to expect on the ballot, to fill in missing values, and to validate that the ballot is well-formed.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_style_id A pointer to the output StyleId.  The caller is responsible for freeing it.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_plaintext_ballot_get_style_id(
	eg_plaintext_ballot_t *handle,
	char **out_style_id
	);

/**
 * @brief The size of the Contests collection.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @return The value of the property
 */
EG_API uint64_t eg_plaintext_ballot_get_contests_size(
	eg_plaintext_ballot_t *handle
	);

/**
 * Get the contest at the specified index.
 * @param[in] in_index The index of the contest
 * @param[out] out_get_contest_at_index_ref An opaque pointer to the PlaintextBallotContest
 *                               The value is a reference and is not owned by the caller
 */
EG_API eg_electionguard_status_t eg_plaintext_ballot_get_contest_at_index(
	eg_plaintext_ballot_t *handle,
	uint64_t in_index,
	eg_plaintext_ballot_contest_t **out_get_contest_at_index_ref
	);

/**
 * Export the ballot representation as JSON
 */
EG_API eg_electionguard_status_t eg_plaintext_ballot_to_json(
	eg_plaintext_ballot_t *handle,
	char **out_data,
	uint64_t *out_size
	);

/**
 * Frees the memory held by the PlaintextBallot
 */
EG_API eg_electionguard_status_t eg_plaintext_ballot_free(eg_plaintext_ballot_t *handle);

#endif // ifndef PlaintextBallot

#ifdef __cplusplus
}
#endif
