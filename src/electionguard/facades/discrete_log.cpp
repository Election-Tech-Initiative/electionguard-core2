#include "electionguard/discrete_log.hpp"

#include "../log.hpp"
#include "electionguard/group.hpp"
#include "electionguard/status.h"
#include "variant_cast.hpp"

extern "C" {
#include "electionguard/discrete_log.h"
}

using electionguard::DiscreteLog;
using electionguard::ElementModP;
using electionguard::Log;

#pragma region DiscreteLog

eg_electionguard_status_t eg_discrete_log_get_async(eg_element_mod_p_t *in_element,
                                                    uint64_t *out_result)
{
    if (in_element == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    try {
        auto *element = AS_TYPE(ElementModP, in_element);
        *out_result = DiscreteLog::getAsync(*element);
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(":eg_discrete_log_get_async", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion
