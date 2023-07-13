#ifndef __ELECTIONGUARD_CPP_HASH_H_INCLUDED__
#define __ELECTIONGUARD_CPP_HASH_H_INCLUDED__

#include "export.h"
#include "group.h"
#include "status.h"

#ifdef __cplusplus
extern "C" {
#endif

struct eg_crypto_hashable_type_s;
typedef struct eg_crypto_hashable_type_s eg_crypto_hashable_type_t;

// EG_API eg_electionguard_status_t eg_hash_elems(const char *prefix,
//                                                eg_crypto_hashable_type_t *in_data[],
//                                                uint64_t in_data_size,
//                                                eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string(const char *a,
                                                      eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_strings(const char **a, uint64_t in_length,
                                                       eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_int(const uint64_t a,
                                                   eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string_int(char *a, uint64_t b,
                                                          eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string_int_modp(char *a, uint64_t b,
                                                               eg_element_mod_p_t *c,
                                                               eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string_int_modq(char *a, uint64_t b,
                                                               eg_element_mod_q_t *c,
                                                               eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_modp_modp(eg_element_mod_p_t *a,
                                                         eg_element_mod_p_t *b,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_modp_modq(eg_element_mod_p_t *a,
                                                         eg_element_mod_q_t *b,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_modq_modq(eg_element_mod_q_t *a,
                                                         eg_element_mod_q_t *b,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_modq_modp(eg_element_mod_q_t *a,
                                                         eg_element_mod_p_t *b,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_array(eg_element_mod_p_t *in_data[],
                                                     uint64_t in_data_size,
                                                     eg_element_mod_q_t **out_handle);

#ifdef __cplusplus
}
#endif
#endif /* __ELECTIONGUARD_CPP_HASH_H_INCLUDED__ */
