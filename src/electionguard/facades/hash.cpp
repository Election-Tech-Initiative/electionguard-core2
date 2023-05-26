#include "electionguard/hash.hpp"

#include "../log.hpp"
#include "convert.hpp"
#include "electionguard/group.hpp"
#include "variant_cast.hpp"

extern "C" {
#include "electionguard/hash.h"
}

using electionguard::CryptoHashableType;
using electionguard::ElementModP;
using electionguard::ElementModQ;
using electionguard::hash_elems;
using electionguard::Log;
using electionguard::uint64_to_size;
using electionguard::variant_cast;
using std::vector;

eg_electionguard_status_t eg_hash_elems_string(const char *a, eg_element_mod_q_t **out_handle)
{
    try {
        auto hash = hash_elems(a);
        *out_handle = AS_TYPE(eg_element_mod_q_t, hash.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(": eg_hash_elems_string", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hash_elems_strings(const char **a, uint64_t in_length,
                                                eg_element_mod_q_t **out_handle)
{
    try {
        vector<string> v(a, a + in_length);
        auto hash = hash_elems(v);
        *out_handle = AS_TYPE(eg_element_mod_q_t, hash.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(": eg_hash_elems_strings", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hash_elems_int(const uint64_t a, eg_element_mod_q_t **out_handle)
{
    try {
        auto hash = hash_elems(a);
        *out_handle = AS_TYPE(eg_element_mod_q_t, hash.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(": eg_hash_elems_int", e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hash_elems_string_int(char *in_first, uint64_t in_second,
                                                   eg_element_mod_q_t **out_handle)
{
    try {
        auto first = string(in_first);
        auto result = hash_elems({first, in_second});

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hash_elems_string_int_modp(char *in_first, uint64_t in_second,
                                                        eg_element_mod_p_t *c,
                                                        eg_element_mod_q_t **out_handle)
{
    try {
        auto first = string(in_first);
        auto *third = AS_TYPE(ElementModP, c);
        auto result = hash_elems({first, in_second, third});

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hash_elems_string_int_modq(char *in_first, uint64_t in_second,
                                                        eg_element_mod_q_t *c,
                                                        eg_element_mod_q_t **out_handle)
{
    try {
        auto first = string(in_first);
        auto *third = AS_TYPE(ElementModQ, c);
        auto result = hash_elems({first, in_second, third});

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_hash_elems_modp_modp(eg_element_mod_p_t *a,
                                                         eg_element_mod_p_t *b,
                                                         eg_element_mod_q_t **out_handle)
{
    try {
        auto *first = AS_TYPE(ElementModP, a);
        auto *second = AS_TYPE(ElementModP, b);
        auto result = hash_elems({first, second});

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hash_elems_modp_modq(eg_element_mod_p_t *a, eg_element_mod_q_t *b,
                                                  eg_element_mod_q_t **out_handle)
{
    try {
        auto *first = AS_TYPE(ElementModP, a);
        auto *second = AS_TYPE(ElementModQ, b);
        auto result = hash_elems({first, second});

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_hash_elems_modq_modq(eg_element_mod_q_t *a,
                                                         eg_element_mod_q_t *b,
                                                         eg_element_mod_q_t **out_handle)
{
    try {
        auto *first = AS_TYPE(ElementModQ, a);
        auto *second = AS_TYPE(ElementModQ, b);
        auto result = hash_elems({first, second});

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hash_elems_modq_modp(eg_element_mod_q_t *a, eg_element_mod_p_t *b,
                                                  eg_element_mod_q_t **out_handle)
{
    try {
        auto *first = AS_TYPE(ElementModQ, a);
        auto *second = AS_TYPE(ElementModP, b);
        auto result = hash_elems({first, second});

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hash_elems_array(eg_element_mod_p_t *in_data[], uint64_t in_data_size,
                                              eg_element_mod_q_t **out_handle)
{
    try {
        vector<ElementModP *> elements;
        elements.reserve(uint64_to_size(in_data_size));
        for (uint64_t i = 0; i < in_data_size; i++) {
            auto *element = AS_TYPE(ElementModP, in_data[i]);
            elements.push_back(element);
        }
        auto result = hash_elems(elements);

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion
