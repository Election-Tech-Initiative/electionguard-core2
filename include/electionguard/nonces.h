#ifndef __ELECTIONGUARD_CPP_NONCES_H_INCLUDED__
#define __ELECTIONGUARD_CPP_NONCES_H_INCLUDED__

#include "export.h"
#include "group.h"
#include "status.h"

#ifdef __cplusplus
extern "C" {
#endif

struct eg_nonce_sequence_s;
typedef struct eg_nonce_sequence_s eg_nonce_sequence_t;

EG_API eg_electionguard_status_t eg_nonce_sequence_new(eg_element_mod_q_t *in_seed,
                                                       eg_nonce_sequence_t **out_handle);

EG_API eg_electionguard_status_t eg_nonce_sequence_new_with_p_header(
  eg_element_mod_q_t *in_seed, eg_element_mod_p_t *in_header, eg_nonce_sequence_t **out_handle);

EG_API eg_electionguard_status_t eg_nonce_sequence_new_with_q_header(
  eg_element_mod_q_t *in_seed, eg_element_mod_q_t *in_header, eg_nonce_sequence_t **out_handle);

EG_API eg_electionguard_status_t eg_nonce_sequence_new_with_string_header(
  eg_element_mod_q_t *in_seed, const char *in_header, eg_nonce_sequence_t **out_handle);

EG_API eg_electionguard_status_t eg_nonce_sequence_free(eg_nonce_sequence_t *handle);

EG_API eg_electionguard_status_t eg_nonce_sequence_get_item(eg_nonce_sequence_t *handle,
                                                            uint64_t in_item,
                                                            eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t
eg_nonce_sequence_get_item_with_header(eg_nonce_sequence_t *handle, uint64_t in_item,
                                       const char *in_header, eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_nonce_sequence_get_items(eg_nonce_sequence_t *handle,
                                                             uint64_t in_start_item,
                                                             uint64_t in_count,
                                                             eg_element_mod_q_t ***out_handles,
                                                             uint64_t *out_size);

EG_API eg_electionguard_status_t eg_nonce_sequence_next(eg_nonce_sequence_t *handle,
                                                        eg_element_mod_q_t **out_handle);

#ifdef __cplusplus
}
#endif
#endif /* __ELECTIONGUARD_CPP_NONCES_H_INCLUDED__ */
