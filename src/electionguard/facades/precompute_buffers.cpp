#include "electionguard/precompute_buffers.hpp"

#include "../log.hpp"
#include "convert.hpp"
#include "variant_cast.hpp"

#include <cerrno>
#include <exception>
#include <stdexcept>

extern "C" {
#include "electionguard/precompute_buffers.h"
}

using electionguard::ElementModP;
using electionguard::ElementModQ;
using electionguard::PrecomputeBufferContext;

#pragma region Precompute

eg_electionguard_status_t eg_precompute_buffer_context_initialize(eg_element_mod_p_t *in_public_key,
                                                                  uint32_t max_buffers)
{
    try {
        auto *public_key = AS_TYPE(ElementModP, in_public_key);
        PrecomputeBufferContext::initialize(*public_key, max_buffers);
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const std::exception &e) {
        Log::error(":eg_precompute_init", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

eg_electionguard_status_t eg_precompute_buffer_context_start()
{
    try {

        PrecomputeBufferContext::start();
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const std::exception &e) {
        Log::error(":eg_precompute_start", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

eg_electionguard_status_t eg_precompute_buffer_context_start_new(eg_element_mod_p_t *in_public_key)
{
    try {
        auto *public_key = AS_TYPE(ElementModP, in_public_key);
        PrecomputeBufferContext::start(*public_key);
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const std::exception &e) {
        Log::error(":eg_precompute_start_with_new_key", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

eg_electionguard_status_t eg_precompute_buffer_context_stop()
{
    try {
        PrecomputeBufferContext::stop();
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const std::exception &e) {
        Log::error(":eg_precompute_stop", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

eg_electionguard_status_t eg_precompute_buffer_context_status(uint32_t *out_count,
                                                              uint32_t *out_queue_size)
{
    try {
        *out_count = PrecomputeBufferContext::getCurrentQueueSize();
        *out_queue_size = PrecomputeBufferContext::getMaxQueueSize();
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const std::exception &e) {
        Log::error(":eg_precompute_status", e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

#pragma endregion
