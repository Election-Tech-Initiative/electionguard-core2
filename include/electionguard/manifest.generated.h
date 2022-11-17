/// @file election_manifest.generated.h
#pragma once

#include "chaum_pedersen.h"
#include "elgamal.h"
#include "export.h"
#include "group.h"
#include "status.h"
#include "manifest.h"

#ifdef __cplusplus
extern "C" {
#endif

#ifndef Manifest

/**
 * @brief Unique identifier for a GpUnit element. Associates the election with a reporting unit that represents the geographical scope of the election, such as a state or city.
 * @param[in] handle A pointer to the `eg_plaintext_ballot_selection_t` opaque instance
 * @param[out] out_election_scope_id A pointer to the output ElectionScopeId.  The caller is responsible for freeing it.
 * @return eg_electionguard_status_t indicating success or failure
 * @retval ELECTIONGUARD_STATUS_SUCCESS The function was successfully executed
 * @retval ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC The function was unable to allocate memory
 */
EG_API eg_electionguard_status_t eg_election_manifest_get_election_scope_id(
	eg_election_manifest_t *handle,
	char **out_election_scope_id
	);

/**
 * Frees the memory held by the Manifest
 */
EG_API eg_electionguard_status_t eg_election_manifest_free(eg_election_manifest_t *handle);

#endif // ifndef Manifest

#ifdef __cplusplus
}
#endif
