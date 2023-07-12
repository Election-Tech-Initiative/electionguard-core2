/// @file exception_handler.h
#ifndef __ELECTIONGUARD_CPP_EXCEPTION_HANDLER_H_INCLUDED__
#define __ELECTIONGUARD_CPP_EXCEPTION_HANDLER_H_INCLUDED__

#include "export.h"
#include "status.h"

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
EG_API eg_electionguard_status_t eg_exception_data(char **out_function, char **out_message,
                                                   uint64_t *out_code);

#ifdef __cplusplus
}
#endif

#endif // __ELECTIONGUARD_CPP_EXCEPTION_HANDLER_H_INCLUDED__