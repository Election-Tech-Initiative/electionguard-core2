/// @file plaintext_ballot_contest.generated.h
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

#ifndef PlaintextBallotContest

/**
 * @brief Get the objectId of the contest, which is the unique id for the contest in a specific ballot style described in the election manifest.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_object_id A pointer to the output ObjectId.  The caller is responsible for freeing it.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_plaintext_ballot_contest_get_object_id(
	eg_plaintext_ballot_contest_t *handle,
	char **out_object_id
	);

/**
 * @brief Get the Size of the selections collection
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @return The value of the property
 */
EG_API uint64_t eg_plaintext_ballot_contest_get_selections_size(
	eg_plaintext_ballot_contest_t *handle
	);

/**
 * Given a PlaintextBallotContest returns true if the state is representative of the expected values.  Note: because this class supports partial representations, undervotes are considered a valid state.
 */
EG_API bool eg_plaintext_ballot_contest_is_valid(
	eg_plaintext_ballot_contest_t *handle,
	char *in_expected_object_id,
	uint64_t in_expected_num_selections,
	uint64_t in_expected_num_elected,
	uint64_t in_votes_allowed
	);

/**
 * Get a selection at a specific index.
 */
EG_API eg_electionguard_status_t eg_plaintext_ballot_contest_get_selection_at(
	eg_plaintext_ballot_contest_t *handle,
	uint64_t in_index,
	eg_plaintext_ballot_selection_t **out_get_selection_at_ref
	);

EG_API eg_electionguard_status_t eg_plaintext_ballot_contest_free(eg_plaintext_ballot_contest_t *handle);

#endif // ifndef PlaintextBallotContest

#ifdef __cplusplus
}
#endif
