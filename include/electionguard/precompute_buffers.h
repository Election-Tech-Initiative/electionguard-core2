/// @file precompute_buffers.h
#ifndef __ELECTIONGUARD_CPP_PRECOMPUTE_BUFFERS_H_INCLUDED__
#define __ELECTIONGUARD_CPP_PRECOMPUTE_BUFFERS_H_INCLUDED__

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

struct eg_precompute_buffer_s;
typedef struct eg_precompute_buffer_s eg_precompute_buffer_t;

#ifndef PrecomputeBuffer Functions

EG_API eg_electionguard_status_t eg_precompute_buffer_new(eg_element_mod_p_t *in_public_key,
                                                          uint32_t max_buffers,
                                                          bool should_auto_populate,
                                                          eg_precompute_buffer_t **out_handle);

EG_API eg_electionguard_status_t eg_precompute_buffer_free(eg_precompute_buffer_t *in_handle);

EG_API eg_electionguard_status_t eg_precompute_buffer_start(eg_precompute_buffer_t *in_handle);

EG_API eg_electionguard_status_t eg_precompute_buffer_stop(eg_precompute_buffer_t *in_handle);

EG_API eg_electionguard_status_t eg_precompute_buffer_status(eg_precompute_buffer_t *in_handle,
                                                             uint32_t *out_count,
                                                             uint32_t *out_queue_size);

#endif

#ifndef PrecomputeBufferContext Functions

/**
 * @brief A singleton context for a collection of precomputed triples and quadruples.
 *
 * When initializing the context, the caller can specify the maximum number of
 * quadruples to precompute. The number of triples in the triple queue will be
 * twice this.
 *
 * The context is initialized against a specific public key. There can only be one context
 * initialized at a time. If the caller attempts to initialize the context with a different
 * public key, then the context will be cleared and re-initialized.
 *
 * The context is thread safe.
 * 
 * @param in_public_key 
 * @param max_buffers 
 * @return EG_API 
 */
EG_API eg_electionguard_status_t
eg_precompute_buffer_context_initialize(eg_element_mod_p_t *in_public_key, uint32_t max_buffers);

EG_API eg_electionguard_status_t eg_precompute_buffer_context_start();

/**
 * @brief Start a new precompute buffer context.  This is useful for when you want to
 * precompute buffers for multiple elections.  This will clear the existing context
 * and start a new one.
 * 
 * @param in_public_key  the public key to use for precomputing
 */
EG_API eg_electionguard_status_t
eg_precompute_buffer_context_start_new(eg_element_mod_p_t *in_public_key);

EG_API eg_electionguard_status_t eg_precompute_buffer_context_stop();

EG_API eg_electionguard_status_t eg_precompute_buffer_context_status(uint32_t *out_count,
                                                                     uint32_t *out_queue_size);

#endif

#ifdef __cplusplus
}
#endif
#endif /* __ELECTIONGUARD_CPP_PRECOMPUTE_BUFFERS_H_INCLUDED__ */