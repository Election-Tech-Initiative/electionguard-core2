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
 * Frees the memory held by the CiphertextBallot
 */
EG_API eg_electionguard_status_t eg_ciphertext_ballot_free(eg_ciphertext_ballot_t *handle);

#endif // ifndef CiphertextBallot

#ifdef __cplusplus
}
#endif
