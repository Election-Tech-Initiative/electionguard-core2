#include "electionguard/nonces.hpp"

#include "../log.hpp"
#include "electionguard/group.hpp"
#include "variant_cast.hpp"

extern "C" {
#include "electionguard/nonces.h"
}

using electionguard::ElementModP;
using electionguard::ElementModQ;
using electionguard::Log;
using electionguard::Nonces;

using std::make_unique;
using std::vector;

struct eg_nonce_sequence_s;
typedef struct eg_nonce_sequence_s eg_nonce_sequence_t;

eg_electionguard_status_t eg_nonce_sequence_new(eg_element_mod_q_t *in_seed,
                                                eg_nonce_sequence_t **out_handle)
{
    try {
        if (in_seed == nullptr || out_handle == nullptr) {
            return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
        }
        auto seed = *AS_TYPE(ElementModQ, in_seed);
        auto sequence = make_unique<Nonces>(seed);
        *out_handle = AS_TYPE(eg_nonce_sequence_t, sequence.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_nonce_sequence_new_with_p_header(eg_element_mod_q_t *in_seed,
                                                              eg_element_mod_p_t *in_header,
                                                              eg_nonce_sequence_t **out_handle)
{
    try {
        if (in_seed == nullptr || in_header == nullptr || out_handle == nullptr) {
            return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
        }
        auto seed = *AS_TYPE(ElementModQ, in_seed);
        auto header = AS_TYPE(ElementModP, in_header);
        auto sequence = make_unique<Nonces>(seed, header);
        *out_handle = AS_TYPE(eg_nonce_sequence_t, sequence.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_nonce_sequence_new_with_q_header(eg_element_mod_q_t *in_seed,
                                                              eg_element_mod_q_t *in_header,
                                                              eg_nonce_sequence_t **out_handle)
{
    try {
        if (in_seed == nullptr || in_header == nullptr || out_handle == nullptr) {
            return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
        }
        auto seed = *AS_TYPE(ElementModQ, in_seed);
        auto header = AS_TYPE(ElementModQ, in_header);
        auto sequence = make_unique<Nonces>(seed, header);
        *out_handle = AS_TYPE(eg_nonce_sequence_t, sequence.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_nonce_sequence_new_with_string_header(eg_element_mod_q_t *in_seed,
                                                                   const char *in_header,
                                                                   eg_nonce_sequence_t **out_handle)
{
    try {
        if (in_seed == nullptr || in_header == nullptr || out_handle == nullptr) {
            return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
        }
        auto seed = *AS_TYPE(ElementModQ, in_seed);
        auto header = string(in_header);
        auto sequence = make_unique<Nonces>(seed, header);
        *out_handle = AS_TYPE(eg_nonce_sequence_t, sequence.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_nonce_sequence_free(eg_nonce_sequence_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }
    delete AS_TYPE(Nonces, handle);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_nonce_sequence_get_item(eg_nonce_sequence_t *handle, uint64_t in_item,
                                                     eg_element_mod_q_t **out_handle)
{
    try {
        if (handle == nullptr || out_handle == nullptr) {
            return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
        }
        auto sequence = AS_TYPE(Nonces, handle);
        auto item = sequence->get(in_item);
        *out_handle = AS_TYPE(eg_element_mod_q_t, item.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_nonce_sequence_get_item_with_header(eg_nonce_sequence_t *handle,
                                                                 uint64_t in_item,
                                                                 const char *in_headers,
                                                                 eg_element_mod_q_t **out_handle)
{
    try {
        if (handle == nullptr || in_headers == nullptr || out_handle == nullptr) {
            return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
        }
        auto sequence = AS_TYPE(Nonces, handle);
        auto item = sequence->get(in_item, in_headers);
        *out_handle = AS_TYPE(eg_element_mod_q_t, item.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_nonce_sequence_get_items(eg_nonce_sequence_t *handle,
                                                      uint64_t in_start_item, uint64_t in_count,
                                                      eg_element_mod_q_t ***out_handles,
                                                      uint64_t *out_size)
{
    try {
        if (handle == nullptr || out_handles == nullptr || out_size == nullptr) {
            return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
        }
        auto sequence = AS_TYPE(Nonces, handle);
        auto items = sequence->get(in_start_item, in_count);
        *out_size = items.size();
        *out_handles = new eg_element_mod_q_t *[items.size()];
        for (size_t i = 0; i < items.size(); i++) {
            (*out_handles)[i] = AS_TYPE(eg_element_mod_q_t, items[i].release());
        }
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_nonce_sequence_next(eg_nonce_sequence_t *handle,
                                                 eg_element_mod_q_t **out_handle)
{
    try {
        if (handle == nullptr || out_handle == nullptr) {
            return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
        }
        auto sequence = AS_TYPE(Nonces, handle);
        auto item = sequence->next();
        *out_handle = AS_TYPE(eg_element_mod_q_t, item.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}
