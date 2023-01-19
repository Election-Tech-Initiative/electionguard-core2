#include "electionguard/group.hpp"

#include "../log.hpp"
#include "convert.hpp"
#include "electionguard/hash.hpp"
#include "variant_cast.hpp"

extern "C" {
#include "electionguard/group.h"
}
#include "serialize.hpp"

#include <cstring>

using electionguard::bytes_to_p;
using electionguard::bytes_to_q;
using electionguard::dynamicCopy;
using electionguard::ElementModP;
using electionguard::ElementModQ;
using electionguard::G;
using electionguard::hash_elems;
using electionguard::Log;
using electionguard::ONE_MOD_P;
using electionguard::ONE_MOD_Q;
using electionguard::P;
using electionguard::pow_mod_p;
using electionguard::Q;
using electionguard::R;
using electionguard::rand_q;
using electionguard::TWO_MOD_P;
using electionguard::TWO_MOD_Q;
using electionguard::uint64_to_size;
using electionguard::ZERO_MOD_P;
using electionguard::ZERO_MOD_Q;

using std::make_unique;
using std::string;

using ConstantsSerializer = electionguard::Serialize::Constants;

#pragma region ElementModP

eg_electionguard_status_t eg_element_mod_p_new(const uint64_t in_data[MAX_P_LEN],
                                               eg_element_mod_p_t **out_handle)
{
    try {
        uint64_t data[MAX_P_LEN] = {};
        memcpy(static_cast<uint64_t *>(data), in_data, MAX_P_SIZE);

        auto element = make_unique<ElementModP>(data);
        *out_handle = AS_TYPE(eg_element_mod_p_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_element_mod_p_new_unchecked(const uint64_t in_data[MAX_P_LEN],
                                                         eg_element_mod_p_t **out_handle)
{
    try {
        uint64_t data[MAX_P_LEN] = {};
        memcpy(static_cast<uint64_t *>(data), in_data, MAX_P_SIZE);

        auto element = make_unique<ElementModP>(data, true);
        *out_handle = AS_TYPE(eg_element_mod_p_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_p_new_bytes(uint8_t *in_data, uint64_t in_size,
                                                            eg_element_mod_p_t **out_handle)
{
    try {
        auto element = bytes_to_p(in_data, in_size);
        *out_handle = AS_TYPE(eg_element_mod_p_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_element_mod_p_free(eg_element_mod_p_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(ElementModP, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_p_get_data(eg_element_mod_p_t *handle, uint64_t **out_data,
                                                    uint64_t *out_size)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto *element = AS_TYPE(ElementModP, handle);
    *out_data = element->get();
    *out_size = (uint64_t)MAX_P_LEN;

    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_p_to_hex(eg_element_mod_p_t *handle, char **out_hex)
{
    try {
        auto hex_rep = AS_TYPE(ElementModP, handle)->toHex();
        *out_hex = dynamicCopy(hex_rep);

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_element_mod_p_to_bytes(eg_element_mod_p_t *handle, uint8_t **out_bytes,
                                                    size_t *out_size)
{
    try {
        auto hex_rep = AS_TYPE(ElementModP, handle)->toBytes();

        size_t size = 0;
        *out_bytes = dynamicCopy(hex_rep, &size);
        *out_size = (uint64_t)size;

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion

#pragma region ElementModQ

eg_electionguard_status_t eg_element_mod_q_new(const uint64_t in_data[MAX_Q_LEN],
                                               eg_element_mod_q_t **out_handle)
{
    try {
        uint64_t data[MAX_Q_LEN] = {};
        memcpy(static_cast<uint64_t *>(data), in_data, MAX_Q_SIZE);

        auto element = make_unique<ElementModQ>(data);
        *out_handle = AS_TYPE(eg_element_mod_q_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_element_mod_q_new_unchecked(const uint64_t in_data[MAX_Q_LEN],
                                                         eg_element_mod_q_t **out_handle)
{
    try {
        uint64_t data[MAX_Q_LEN] = {};
        memcpy(static_cast<uint64_t *>(data), in_data, MAX_Q_SIZE);

        auto element = make_unique<ElementModQ>(data, true);
        *out_handle = AS_TYPE(eg_element_mod_q_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_q_new_bytes(uint8_t *in_data, uint64_t in_size,
                                                            eg_element_mod_q_t **out_handle)
{
    try {
        auto element = bytes_to_q(in_data, in_size);
        *out_handle = AS_TYPE(eg_element_mod_q_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_element_mod_q_free(eg_element_mod_q_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(ElementModQ, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_q_get_data(eg_element_mod_q_t *handle, uint64_t **out_data,
                                                    uint64_t *out_size)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    auto *element = AS_TYPE(ElementModQ, handle);
    *out_data = element->get();
    *out_size = (uint64_t)MAX_Q_LEN;

    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_q_to_bytes(eg_element_mod_q_t *handle, uint8_t **out_bytes,
                                                    uint64_t *out_size)
{
    try {
        auto hex_rep = AS_TYPE(ElementModQ, handle)->toBytes();

        size_t size = 0;
        *out_bytes = dynamicCopy(hex_rep, &size);
        *out_size = (uint64_t)size;

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_element_mod_q_to_hex(eg_element_mod_q_t *handle, char **out_hex)
{
    try {
        auto hex_rep = AS_TYPE(ElementModQ, handle)->toHex();
        *out_hex = dynamicCopy(hex_rep);

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_q_from_hex(char *in_hex,
                                                           eg_element_mod_q_t **out_handle)
{
    try {
        auto element = ElementModQ::fromHex(string(in_hex), false);
        *out_handle = AS_TYPE(eg_element_mod_q_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t
eg_element_mod_q_from_hex_unchecked(char *in_hex, eg_element_mod_q_t **out_handle)
{
    try {
        auto element = ElementModQ::fromHex(string(in_hex), true);
        *out_handle = AS_TYPE(eg_element_mod_q_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_q_from_uint64(uint64_t in_uint64,
                                                              eg_element_mod_q_t **out_handle)
{
    try {
        auto element = ElementModQ::fromUint64(in_uint64, false);
        *out_handle = AS_TYPE(eg_element_mod_q_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t
eg_element_mod_q_from_uint64_unchecked(uint64_t in_uint64, eg_element_mod_q_t **out_handle)
{
    try {
        auto element = ElementModQ::fromUint64(in_uint64, true);
        *out_handle = AS_TYPE(eg_element_mod_q_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_p_from_uint64(uint64_t in_uint64,
                                                              eg_element_mod_p_t **out_handle)
{
    try {
        auto element = ElementModP::fromUint64(in_uint64, false);
        *out_handle = AS_TYPE(eg_element_mod_p_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t
eg_element_mod_p_from_uint64_unchecked(uint64_t in_uint64, eg_element_mod_p_t **out_handle)
{
    try {
        auto element = ElementModP::fromUint64(in_uint64, true);
        *out_handle = AS_TYPE(eg_element_mod_p_t, element.release());

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion

#pragma region Group Constants

eg_electionguard_status_t eg_element_mod_p_constant_g(eg_element_mod_p_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_p_t, const_cast<ElementModP *>(&G()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_p_constant_p(eg_element_mod_p_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_p_t, const_cast<ElementModP *>(&P()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_p_constant_r(eg_element_mod_p_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_p_t, const_cast<ElementModP *>(&R()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_element_mod_p_constant_zero_mod_p(eg_element_mod_p_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_p_t, const_cast<ElementModP *>(&ZERO_MOD_P()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_p_constant_one_mod_p(eg_element_mod_p_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_p_t, const_cast<ElementModP *>(&ONE_MOD_P()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_p_constant_two_mod_p(eg_element_mod_p_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_p_t, const_cast<ElementModP *>(&TWO_MOD_P()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_q_constant_q(eg_element_mod_q_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_q_t, const_cast<ElementModQ *>(&Q()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_element_mod_q_constant_zero_mod_q(eg_element_mod_q_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_q_t, const_cast<ElementModQ *>(&ZERO_MOD_Q()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_q_constant_one_mod_q(eg_element_mod_q_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_q_t, const_cast<ElementModQ *>(&ONE_MOD_Q()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_element_mod_q_constant_two_mod_q(eg_element_mod_q_t **out_constant_ref)
{
    *out_constant_ref = AS_TYPE(eg_element_mod_q_t, const_cast<ElementModQ *>(&TWO_MOD_Q()));
    return ELECTIONGUARD_STATUS_SUCCESS;
}

#pragma endregion

#pragma region Group Math Functions

eg_electionguard_status_t eg_element_mod_q_pow_mod_p(eg_element_mod_p_t *base,
                                                     eg_element_mod_q_t *exponent,
                                                     eg_element_mod_p_t **out_handle)
{
    try {
        auto *b = AS_TYPE(ElementModP, base);
        auto *e = AS_TYPE(ElementModQ, exponent);
        auto result = pow_mod_p(*b, *e);

        *out_handle = AS_TYPE(eg_element_mod_p_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_long_pow_mod_p(uint64_t base, uint64_t exponent,
                                                           eg_element_mod_p_t **out_handle)
{
    try {
        auto b = ElementModP::fromUint64(base);
        auto e = ElementModP::fromUint64(exponent);
        auto result = pow_mod_p(*b, *e);

        *out_handle = AS_TYPE(eg_element_mod_p_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_element_mod_q_rand_q_new(eg_element_mod_q_t **out_handle)
{
    try {
        auto random = rand_q();
        *out_handle = AS_TYPE(eg_element_mod_q_t, random.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_q_add_mod_q(eg_element_mod_q_t *lhs,
                                                            eg_element_mod_q_t *rhs,
                                                            eg_element_mod_q_t **out_handle)
{
    try {
        auto *l = AS_TYPE(ElementModQ, lhs);
        auto *r = AS_TYPE(ElementModQ, rhs);
        auto result = add_mod_q(*l, *r);

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_p_add_mod_p(eg_element_mod_p_t *lhs,
                                                            eg_element_mod_p_t *rhs,
                                                            eg_element_mod_p_t **out_handle)
{
    try {
        auto *l = AS_TYPE(ElementModP, lhs);
        auto *r = AS_TYPE(ElementModP, rhs);
        auto result = add_mod_p(*l, *r);

        *out_handle = AS_TYPE(eg_element_mod_p_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t
eg_element_mod_q_a_plus_b_mul_c_mod_q(eg_element_mod_q_t *a, eg_element_mod_q_t *b,
                                      eg_element_mod_q_t *c, eg_element_mod_q_t **out_handle)
{
    try {
        auto *a_local = AS_TYPE(ElementModQ, a);
        auto *b_local = AS_TYPE(ElementModQ, b);
        auto *c_local = AS_TYPE(ElementModQ, c);
        auto result = a_plus_bc_mod_q(*a_local, *b_local, *c_local);

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_q_mult_mod_q(eg_element_mod_q_t *lhs,
                                                             eg_element_mod_q_t *rhs,
                                                             eg_element_mod_q_t **out_handle)
{
    try {
        auto *l = AS_TYPE(ElementModQ, lhs);
        auto *r = AS_TYPE(ElementModQ, rhs);
        auto result = mul_mod_q(*l, *r);

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_q_pow_mod_q(eg_element_mod_q_t *base,
                                                            eg_element_mod_q_t *exponent,
                                                            eg_element_mod_q_t **out_handle)
{
    try {
        auto *b = AS_TYPE(ElementModQ, base);
        auto *e = AS_TYPE(ElementModQ, exponent);
        auto result = pow_mod_q(*b, *e);

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_long_pow_mod_q(eg_element_mod_q_t *base,
                                                           uint64_t exponent,
                                                           eg_element_mod_q_t **out_handle)
{
    try {
        auto *b = AS_TYPE(ElementModQ, base);
        auto e = ElementModQ::fromUint64(exponent);
        auto result = pow_mod_q(*b, *e);

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_p_mult_mod_p(eg_element_mod_p_t *lhs,
                                                             eg_element_mod_p_t *rhs,
                                                             eg_element_mod_p_t **out_handle)
{
    try {
        auto *l = AS_TYPE(ElementModP, lhs);
        auto *r = AS_TYPE(ElementModP, rhs);
        auto result = mul_mod_p(*l, *r);

        *out_handle = AS_TYPE(eg_element_mod_p_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_element_mod_p_pow_mod_p(eg_element_mod_p_t *b,
                                                            eg_element_mod_p_t *e,
                                                            eg_element_mod_p_t **out_handle)
{
    try {
        auto *b_local = AS_TYPE(ElementModP, b);
        auto *e_local = AS_TYPE(ElementModP, e);
        auto result = pow_mod_p(*b_local, *e_local);

        *out_handle = AS_TYPE(eg_element_mod_p_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_hash_elems_modp_modp(eg_element_mod_p_t *publickey,
                                                         eg_element_mod_p_t *commitment,
                                                         eg_element_mod_q_t **out_handle)
{
    try {
        auto *p = AS_TYPE(ElementModP, publickey);
        auto *c = AS_TYPE(ElementModP, commitment);
        auto result = hash_elems({p, c});

        *out_handle = AS_TYPE(eg_element_mod_q_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_hash_elems_string_int(char *in_first, uint64_t in_second,
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

EG_API eg_electionguard_status_t eg_hash_elems_array(eg_element_mod_p_t *in_data[],
                                                     uint64_t in_data_size,
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

#pragma region Group Serialization Functions

eg_electionguard_status_t eg_constant_to_json(char **out_data, uint64_t *out_size)
{
    try {
        auto result = ConstantsSerializer::toJson();

        size_t size = 0;
        *out_data = dynamicCopy(result, &size);
        *out_size = (uint64_t)size;

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

#pragma endregion
