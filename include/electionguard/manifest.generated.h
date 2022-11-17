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
 * Frees the memory held by the Manifest
 */
EG_API eg_electionguard_status_t eg_election_manifest_free(eg_election_manifest_t *handle);

#endif // ifndef Manifest

#ifdef __cplusplus
}
#endif
