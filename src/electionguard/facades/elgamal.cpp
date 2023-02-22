#include "electionguard/elgamal.hpp"

#include "../log.hpp"
#include "./electionguard/constants.h"
#include "convert.hpp"
#include "electionguard/group.hpp"
#include "electionguard/status.h"
#include "variant_cast.hpp"

extern "C" {
#include "electionguard/elgamal.h"
}

using electionguard::dynamicCopy;
using electionguard::ElementModP;
using electionguard::ElementModQ;
using electionguard::ElGamalCiphertext;
using electionguard::ElGamalKeyPair;
using electionguard::HashedElGamalCiphertext;
using electionguard::Log;

using std::make_unique;

#pragma region ElGamalKeyPair

eg_electionguard_status_t eg_elgamal_keypair_from_secret_new(eg_element_mod_q_t *in_secret_key,
                                                             eg_elgamal_keypair_t **out_handle)
{
    if (in_secret_key == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    try {
        auto *secretKey = AS_TYPE(ElementModQ, in_secret_key);
        auto keyPair = ElGamalKeyPair::fromSecret(*secretKey);

        *out_handle = AS_TYPE(eg_elgamal_keypair_t, keyPair.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

EG_API eg_electionguard_status_t eg_elgamal_keypair_from_pair_new(eg_element_mod_q_t *in_secret_key,
                                                                  eg_element_mod_p_t *in_public_key,
                                                                  eg_elgamal_keypair_t **out_handle)
{
    if (in_secret_key == nullptr || in_public_key == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    try {
        auto *secretKey = AS_TYPE(ElementModQ, in_secret_key);
        auto *publicKey = AS_TYPE(ElementModP, in_public_key);
        auto keyPair = ElGamalKeyPair::fromPair(*secretKey, *publicKey);

        *out_handle = AS_TYPE(eg_elgamal_keypair_t, keyPair.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_elgamal_keypair_free(eg_elgamal_keypair_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(ElGamalKeyPair, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_elgamal_keypair_get_secret_key(eg_elgamal_keypair_t *handle,
                                                            eg_element_mod_q_t **out_secret_key)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }
    auto *keyPair = AS_TYPE(ElGamalKeyPair, handle);
    *out_secret_key = AS_TYPE(eg_element_mod_q_t, keyPair->getSecretKey());
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_elgamal_keypair_get_public_key(eg_elgamal_keypair_t *handle,
                                                            eg_element_mod_p_t **out_public_key)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }
    auto *keyPair = AS_TYPE(ElGamalKeyPair, handle);
    *out_public_key = AS_TYPE(eg_element_mod_p_t, keyPair->getPublicKey());
    return ELECTIONGUARD_STATUS_SUCCESS;
}

#pragma endregion

#pragma region ElGamalCiphertext

eg_electionguard_status_t eg_elgamal_ciphertext_free(eg_elgamal_ciphertext_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(ElGamalCiphertext, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_elgamal_ciphertext_get_pad(eg_elgamal_ciphertext_t *handle,
                                                        eg_element_mod_p_t **out_pad)
{
    auto *pad = AS_TYPE(ElGamalCiphertext, handle)->getPad();
    *out_pad = AS_TYPE(eg_element_mod_p_t, pad);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_elgamal_ciphertext_get_data(eg_elgamal_ciphertext_t *handle,
                                                         eg_element_mod_p_t **out_data)
{
    auto *data = AS_TYPE(ElGamalCiphertext, handle)->getData();
    *out_data = AS_TYPE(eg_element_mod_p_t, data);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t eg_elgamal_ciphertext_crypto_hash(eg_elgamal_ciphertext_t *handle,
                                                            eg_element_mod_q_t **out_crypto_hash)
{
    try {
        auto cryptoHash = AS_TYPE(ElGamalCiphertext, handle)->crypto_hash();
        *out_crypto_hash = AS_TYPE(eg_element_mod_q_t, cryptoHash.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_elgamal_ciphertext_decrypt_with_secret(
  eg_elgamal_ciphertext_t *handle, eg_element_mod_q_t *in_secret_key, uint64_t *out_plaintext)
{
    try {
        auto *secretKey = AS_TYPE(ElementModQ, in_secret_key);
        *out_plaintext = AS_TYPE(ElGamalCiphertext, handle)->decrypt(*secretKey);
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

#pragma endregion

#pragma region ElgamalEncrypt

eg_electionguard_status_t eg_elgamal_encrypt(uint64_t in_plaintext, eg_element_mod_q_t *in_nonce,
                                             eg_element_mod_p_t *in_public_key,
                                             eg_elgamal_ciphertext_t **out_ciphertext)
{
    try {
        auto *nonce = AS_TYPE(ElementModQ, in_nonce);
        auto *publicKey = AS_TYPE(ElementModP, in_public_key);
        auto ciphertext = elgamalEncrypt(in_plaintext, *nonce, *publicKey);
        *out_ciphertext = AS_TYPE(eg_elgamal_ciphertext_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

#pragma region HashedElGamalCiphertext

eg_electionguard_status_t
eg_hashed_elgamal_ciphertext_new(eg_element_mod_p_t *in_pad, uint8_t *in_data,
                                 uint64_t in_data_length, uint8_t *in_mac, uint64_t in_mac_length,
                                 eg_hashed_elgamal_ciphertext_t **out_handle)
{
    try {
        auto *pad = AS_TYPE(ElementModP, in_pad);
        auto data_bytes = vector<uint8_t>(in_data, in_data + in_data_length);
        auto mac_bytes = vector<uint8_t>(in_mac, in_mac + in_mac_length);
        auto result = HashedElGamalCiphertext::make(*pad, data_bytes, mac_bytes);
        *out_handle = AS_TYPE(eg_hashed_elgamal_ciphertext_t, result.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

eg_electionguard_status_t eg_hashed_elgamal_ciphertext_free(eg_hashed_elgamal_ciphertext_t *handle)
{
    if (handle == nullptr) {
        return ELECTIONGUARD_STATUS_ERROR_INVALID_ARGUMENT;
    }

    delete AS_TYPE(HashedElGamalCiphertext, handle); // NOLINT(cppcoreguidelines-owning-memory)
    handle = nullptr;
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_hashed_elgamal_ciphertext_get_pad(eg_hashed_elgamal_ciphertext_t *handle,
                                     eg_element_mod_p_t **out_pad)
{
    auto *pad = AS_TYPE(HashedElGamalCiphertext, handle)->getPad();
    *out_pad = AS_TYPE(eg_element_mod_p_t, pad);
    return ELECTIONGUARD_STATUS_SUCCESS;
}

eg_electionguard_status_t
eg_hashed_elgamal_ciphertext_get_data(eg_hashed_elgamal_ciphertext_t *handle, uint8_t **out_data,
                                      uint64_t *out_size)
{
    try {
        std::vector<uint8_t> data = AS_TYPE(HashedElGamalCiphertext, handle)->getData();

        size_t size = 0;
        *out_data = dynamicCopy(data, &size);
        *out_size = (uint64_t)size;

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t
eg_hashed_elgamal_ciphertext_get_mac(eg_hashed_elgamal_ciphertext_t *handle, uint8_t **out_data,
                                     uint64_t *out_size)
{
    try {
        std::vector<uint8_t> data = AS_TYPE(HashedElGamalCiphertext, handle)->getMac();

        size_t size = 0;
        *out_data = dynamicCopy(data, &size);
        *out_size = (uint64_t)size;

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t
eg_hashed_elgamal_ciphertext_crypto_hash(eg_hashed_elgamal_ciphertext_t *handle,
                                         eg_element_mod_q_t **out_crypto_hash)
{
    try {
        auto cryptoHash = AS_TYPE(HashedElGamalCiphertext, handle)->crypto_hash();
        *out_crypto_hash = AS_TYPE(eg_element_mod_q_t, cryptoHash.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_BAD_ALLOC;
    }
}

eg_electionguard_status_t eg_hashed_elgamal_ciphertext_decrypt_with_secret(
  eg_hashed_elgamal_ciphertext_t *handle, eg_element_mod_q_t *in_secret_key,
  eg_element_mod_q_t *in_description_hash, bool in_look_for_padding, uint8_t **out_data,
  uint64_t *out_size)
{
    try {
        auto *secretKey = AS_TYPE(ElementModQ, in_secret_key);
        auto *descriptionHash = AS_TYPE(ElementModQ, in_description_hash);
        auto result = AS_TYPE(HashedElGamalCiphertext, handle)
                        ->decrypt(*secretKey, *descriptionHash, in_look_for_padding);

        size_t size = 0;
        *out_data = dynamicCopy(result, &size);
        *out_size = (uint64_t)size;

        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

#pragma endregion

#pragma region HashedElgamalEncrypt

eg_electionguard_status_t eg_hashed_elgamal_encrypt(uint8_t *in_plaintext, uint64_t in_length,
                                                    eg_element_mod_q_t *in_nonce,
                                                    eg_element_mod_p_t *in_public_key,
                                                    eg_element_mod_q_t *in_seed,
                                                    eg_hashed_elgamal_ciphertext_t **out_ciphertext)
{
    try {
        auto data_bytes = vector<uint8_t>(in_plaintext, in_plaintext + in_length);
        auto *nonce = AS_TYPE(ElementModQ, in_nonce);
        auto *publicKey = AS_TYPE(ElementModP, in_public_key);
        auto *seed = AS_TYPE(ElementModQ, in_seed);
        auto ciphertext =
          hashedElgamalEncrypt(data_bytes, *nonce, *publicKey, *seed,
                               electionguard::padded_data_size_t::NO_PADDING, false);
        *out_ciphertext = AS_TYPE(eg_hashed_elgamal_ciphertext_t, ciphertext.release());
        return ELECTIONGUARD_STATUS_SUCCESS;
    } catch (const exception &e) {
        Log::error(__func__, e);
        return ELECTIONGUARD_STATUS_ERROR_RUNTIME_ERROR;
    }
}

#pragma endregion
