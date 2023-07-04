/// @file discrete_log.h
#ifndef __ELECTIONGUARD_CPP_DISCRETE_LOG_H_INCLUDED__
#define __ELECTIONGUARD_CPP_DISCRETE_LOG_H_INCLUDED__

#include "export.h"
#include "group.h"
#include "status.h"

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/**
 * @brief Get the discrete log of an element in a group (assumes G is the base)
 * 
 * @param[in] in_element the element to get the discrete log of 
 * @param[out] out_result the result of the discrete log
 * @return EG_API eg_electionguard_status_t indicating success or failure
 */
EG_API eg_electionguard_status_t eg_discrete_log_get_async_base_g(eg_element_mod_p_t *in_element,
                                                                  uint64_t *out_result);

/**
 * @brief Get the discrete log value for the given element using the specified base
 * This override allows for the caller to specify the base for exponentiations.
 * This is useful for cases where the caller is using a different generator than G()
 * or when encrypting ballots using the base-K ElGamal method.
 *
 * Since this class is a singleton, the base is cached and the cache is cleared
 * when the base changes.
 * 
 * @param[in] in_element the element to get the discrete log of
 * @param[in] in_encryption_base the base to use for exponentiations
 * @param[out] out_result the result of the discrete log
 * @return EG_API eg_electionguard_status_t indicating success or failure
 */
EG_API eg_electionguard_status_t eg_discrete_log_get_async(eg_element_mod_p_t *in_element,
                                                           eg_element_mod_p_t *in_encryption_base,
                                                           uint64_t *out_result);

#ifdef __cplusplus
}
#endif

#endif // __ELECTIONGUARD_CPP_DISCRETE_LOG_H_INCLUDED__