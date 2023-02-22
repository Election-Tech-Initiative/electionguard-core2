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
 * @brief Get the discrete log of an element in a group
 * 
 * @param[in] in_element the element to get the discrete log of 
 * @param[out] out_result the result of the discrete log
 * @return EG_API eg_electionguard_status_t indicating success or failure
 */
EG_API eg_electionguard_status_t eg_discrete_log_get_async(eg_element_mod_p_t *in_element,
                                                           uint64_t *out_result);

#ifdef __cplusplus
}
#endif

#endif // __ELECTIONGUARD_CPP_DISCRETE_LOG_H_INCLUDED__