#include "electionguard/exception_handler.hpp"

#include "convert.hpp"
#include "electionguard/status.h"

extern "C" {
#include "electionguard/exception_handler.h"
}

using electionguard::dynamicCopy;
using electionguard::ExceptionHandler;

#pragma region ExceptionHandler

eg_electionguard_status_t eg_exception_data(char **out_function, char **out_message,
                                            uint64_t *out_code)
{
    try {
        *out_function = dynamicCopy(ExceptionHandler::getInstance().getFunction());
        *out_message = dynamicCopy(ExceptionHandler::getInstance().getMessage());
        *out_code = ExceptionHandler::getInstance().getCode();
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion
