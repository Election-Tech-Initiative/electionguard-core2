#include "electionguard/polynomial.hpp"

#include "../log.hpp"
#include "electionguard/group.hpp"
#include "electionguard/status.h"
#include "variant_cast.hpp"

extern "C" {
#include "electionguard/polynomial.h"
}

using electionguard::ElementModQ;
using electionguard::Log;
using electionguard::Polynomial;

using std::vector;

#pragma region Polynomial

eg_electionguard_status_t eg_polynomial_interpolate(eg_element_mod_q_t *in_coordinate,
                                                    eg_element_mod_q_t **in_degrees,
                                                    uint64_t in_degrees_size,
                                                    eg_element_mod_q_t **out_result)
{
    if (in_coordinate == nullptr || in_degrees == nullptr || in_degrees_size == 0 ||
        out_result == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    try {
        auto *coordinate = AS_TYPE(ElementModQ, in_coordinate);
        vector<ElementModQ> degrees;
        for (uint64_t i = 0; i < in_degrees_size; i++) {
            degrees.push_back(*AS_TYPE(ElementModQ, in_degrees[i]));
        }

        auto result = Polynomial::interpolate(*coordinate, degrees);
        *out_result = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_polynomial_interpolate", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion
