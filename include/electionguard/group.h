#ifndef __ELECTIONGUARD_CPP_GROUP_H_INCLUDED__
#define __ELECTIONGUARD_CPP_GROUP_H_INCLUDED__

#include "constants.h"
#include "export.h"
#include "status.h"

#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>

#ifdef __cplusplus
extern "C" {
#endif

// ElementModP

#ifndef ElementModP

struct eg_element_mod_p_s;
typedef struct eg_element_mod_p_s eg_element_mod_p_t;

EG_API eg_electionguard_status_t eg_element_mod_p_new(const uint64_t in_data[MAX_P_LEN],
                                                      eg_element_mod_p_t **out_handle);
EG_API eg_electionguard_status_t eg_element_mod_p_new_unchecked(const uint64_t in_data[MAX_P_LEN],
                                                                eg_element_mod_p_t **out_handle);
EG_API eg_electionguard_status_t eg_element_mod_p_new_bytes(uint8_t *in_data, uint64_t in_size,
                                                            eg_element_mod_p_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_p_free(eg_element_mod_p_t *handle);

EG_API eg_electionguard_status_t eg_element_mod_p_get_data(eg_element_mod_p_t *handle,
                                                           uint64_t **out_data, uint64_t *out_size);

EG_API eg_electionguard_status_t eg_element_mod_p_is_valid_residue(eg_element_mod_p_t *modp,
                                                                   bool *out_value);

EG_API eg_electionguard_status_t eg_element_mod_p_is_in_bounds(eg_element_mod_p_t *modp,
                                                               bool *out_value);

EG_API eg_electionguard_status_t eg_element_mod_p_to_bytes(eg_element_mod_p_t *handle,
                                                           uint8_t **out_bytes, uint64_t *out_size);

EG_API eg_electionguard_status_t eg_element_mod_p_to_hex(eg_element_mod_p_t *handle,
                                                         char **out_hex);

// TODO: ISSUE #129: finish implementation

#endif

#ifndef ElementModQ

struct eg_element_mod_q_s;
typedef struct eg_element_mod_q_s eg_element_mod_q_t;

EG_API eg_electionguard_status_t eg_element_mod_q_new(const uint64_t in_data[MAX_Q_LEN],
                                                      eg_element_mod_q_t **out_handle);
EG_API eg_electionguard_status_t eg_element_mod_q_new_unchecked(const uint64_t in_data[MAX_Q_LEN],
                                                                eg_element_mod_q_t **out_handle);
EG_API eg_electionguard_status_t eg_element_mod_q_new_bytes(uint8_t *in_data, uint64_t in_size,
                                                            eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_q_free(eg_element_mod_q_t *handle);

EG_API eg_electionguard_status_t eg_element_mod_q_get_data(eg_element_mod_q_t *handle,
                                                           uint64_t **out_data, uint64_t *out_size);

EG_API eg_electionguard_status_t eg_element_mod_q_is_in_bounds(eg_element_mod_q_t *modp,
                                                               bool *out_value);

EG_API eg_electionguard_status_t eg_element_mod_q_to_hex(eg_element_mod_q_t *handle,
                                                         char **out_hex);

EG_API eg_electionguard_status_t eg_element_mod_q_from_hex(char *in_hex,
                                                           eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_q_to_bytes(eg_element_mod_q_t *handle,
                                                           uint8_t **out_bytes, uint64_t *out_size);

EG_API eg_electionguard_status_t
eg_element_mod_q_from_hex_unchecked(char *in_hex, eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t
eg_element_mod_q_from_hex_unchecked(char *in_hex, eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_q_from_uint64(uint64_t in_uint64,
                                                              eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t
eg_element_mod_q_from_uint64_unchecked(uint64_t in_uint64, eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_p_from_uint64(uint64_t in_uint64,
                                                              eg_element_mod_p_t **out_handle);

EG_API eg_electionguard_status_t
eg_element_mod_p_from_element_mod_q(eg_element_mod_q_t *in_handle, eg_element_mod_p_t **out_handle);

EG_API eg_electionguard_status_t
eg_element_mod_p_from_uint64_unchecked(uint64_t in_uint64, eg_element_mod_p_t **out_handle);

// TODO: ISSUE #129: finish implementation

#endif

#ifndef Group Constants

EG_API eg_electionguard_status_t eg_element_mod_p_constant_g(eg_element_mod_p_t **out_constant_ref);
EG_API eg_electionguard_status_t eg_element_mod_p_constant_p(eg_element_mod_p_t **out_constant_ref);
EG_API eg_electionguard_status_t eg_element_mod_p_constant_r(eg_element_mod_p_t **out_constant_ref);
EG_API eg_electionguard_status_t
eg_element_mod_p_constant_zero_mod_p(eg_element_mod_p_t **out_constant_ref);
EG_API eg_electionguard_status_t
eg_element_mod_p_constant_one_mod_p(eg_element_mod_p_t **out_constant_ref);
EG_API eg_electionguard_status_t
eg_element_mod_p_constant_two_mod_p(eg_element_mod_p_t **out_constant_ref);

EG_API eg_electionguard_status_t eg_element_mod_q_constant_q(eg_element_mod_q_t **out_constant_ref);
EG_API eg_electionguard_status_t
eg_element_mod_q_constant_zero_mod_q(eg_element_mod_q_t **out_constant_ref);
EG_API eg_electionguard_status_t
eg_element_mod_q_constant_one_mod_q(eg_element_mod_q_t **out_constant_ref);
EG_API eg_electionguard_status_t
eg_element_mod_q_constant_two_mod_q(eg_element_mod_q_t **out_constant_ref);
EG_API eg_electionguard_status_t eg_constant_to_json(char **out_data, uint64_t *out_size);

#endif

#ifndef ElementModP Group Math Functions

// TODO: ISSUE #129: finish implementation

EG_API eg_electionguard_status_t eg_element_mod_p_add_mod_p(eg_element_mod_p_t *lhs,
                                                            eg_element_mod_p_t *rhs,
                                                            eg_element_mod_p_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_p_mult_mod_p(eg_element_mod_p_t *lhs,
                                                             eg_element_mod_p_t *rhs,
                                                             eg_element_mod_p_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_p_div_mod_p(eg_element_mod_p_t *numerator,
                                                            eg_element_mod_p_t *denominator,
                                                            eg_element_mod_p_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_p_pow_mod_p(eg_element_mod_p_t *base,
                                                            eg_element_mod_p_t *exponent,
                                                            eg_element_mod_p_t **out_handle);

// TODO: rename to eg_element_mod_p_pow_mod_p_as_uints
EG_API eg_electionguard_status_t eg_element_long_pow_mod_p(uint64_t base, uint64_t exponent,
                                                           eg_element_mod_p_t **out_handle);

// TODO: rename to eg_element_mod_p_pow_mod_p_with_q_exp
EG_API eg_electionguard_status_t eg_element_mod_q_pow_mod_p(eg_element_mod_p_t *base,
                                                            eg_element_mod_q_t *exponent,
                                                            eg_element_mod_p_t **out_handle);

#endif

#ifndef ElementModQ Group Math Functions

EG_API eg_electionguard_status_t eg_element_mod_q_add_mod_q(eg_element_mod_q_t *lhs,
                                                            eg_element_mod_q_t *rhs,
                                                            eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_q_sub_mod_q(eg_element_mod_q_t *lhs,
                                                            eg_element_mod_q_t *rhs,
                                                            eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_q_mult_mod_q(eg_element_mod_q_t *lhs,
                                                             eg_element_mod_q_t *rhs,
                                                             eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_q_div_mod_q(eg_element_mod_q_t *numerator,
                                                            eg_element_mod_q_t *denominator,
                                                            eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_q_pow_mod_q(eg_element_mod_q_t *base,
                                                            eg_element_mod_q_t *exponent,
                                                            eg_element_mod_q_t **out_handle);

// TODO: rename to eg_element_mod_q_pow_mod_q_with_long_exp
EG_API eg_electionguard_status_t eg_element_long_pow_mod_q(eg_element_mod_q_t *base,
                                                           uint64_t exponent,
                                                           eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t
eg_element_mod_q_a_plus_b_mul_c_mod_q(eg_element_mod_q_t *a, eg_element_mod_q_t *b,
                                      eg_element_mod_q_t *c, eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_element_mod_q_rand_q_new(eg_element_mod_q_t **out_handle);

#endif

#ifndef ElementModP Group Hash Functions

EG_API eg_electionguard_status_t eg_hash_elems_modp_modp(eg_element_mod_p_t *publickey,
                                                         eg_element_mod_p_t *commitment,
                                                         eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_string_int(char *publickey, uint64_t commitment,
                                                          eg_element_mod_q_t **out_handle);

EG_API eg_electionguard_status_t eg_hash_elems_array(eg_element_mod_p_t *in_data[],
                                                     uint64_t in_data_size,
                                                     eg_element_mod_q_t **out_handle);

#endif

#ifdef __cplusplus
}
#endif
#endif /* __ELECTIONGUARD_CPP_GROUP_H_INCLUDED__ */
