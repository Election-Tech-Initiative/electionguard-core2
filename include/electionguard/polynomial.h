/// @file polynomial.h
#ifndef __ELECTIONGUARD_CPP_POLYNOMIAL_H_INCLUDED__
#define __ELECTIONGUARD_CPP_POLYNOMIAL_H_INCLUDED__

#include "export.h"
#include "group.h"
#include "status.h"

#ifdef __cplusplus
extern "C" {

#endif

#ifndef Polynomial

EG_API eg_electionguard_status_t eg_polynomial_interpolate(eg_element_mod_q_t *in_coordinate,
                                                           eg_element_mod_q_t **in_degrees,
                                                           uint64_t in_degrees_size,
                                                           eg_element_mod_q_t **out_result);

#endif

#ifdef __cplusplus
}
#endif

#endif // __ELECTIONGUARD_CPP_POLYNOMIAL_H_INCLUDED__