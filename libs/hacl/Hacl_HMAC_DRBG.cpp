#include "Hacl_HMAC_DRBG.hpp"

#include "Hacl_HMAC_DRBG.h"

namespace hacl
{
    Spec_Hash_Definitions_hash_alg getHashSpecDefinition(HMAC_DRBG_HashAlgorithm algorithm)
    {
        switch (algorithm) {

            case HMAC_DRBG_HashAlgorithm::SHA2_384:
                return Spec_Hash_Definitions_SHA2_384;

            case HMAC_DRBG_HashAlgorithm::SHA2_512:
                return Spec_Hash_Definitions_SHA2_512;

            case HMAC_DRBG_HashAlgorithm::LEGACY_SHA1:
                return Spec_Hash_Definitions_SHA1;
            case HMAC_DRBG_HashAlgorithm::SHA2_256:
            default:
                return Spec_Hash_Definitions_SHA2_256;
        }
    }

    struct HMAC_DRBG::Impl {
        HMAC_DRBG_HashAlgorithm algorithm;
        Spec_Hash_Definitions_hash_alg definition;
        Hacl_HMAC_DRBG_state state;

        Impl(HMAC_DRBG_HashAlgorithm algorithm) : algorithm(algorithm)
        {
            definition = getHashSpecDefinition(algorithm);
            state = Hacl_HMAC_DRBG_create_in(definition);
        }

        void cleanup()
        {
            free(state.k);
            free(state.v);
            free(state.reseed_counter);
        }
    };

    HMAC_DRBG::HMAC_DRBG(
      HMAC_DRBG_HashAlgorithm algorithm /* = HMAC_DRBG_HashAlgorithm::SHA2_256 */)
        : pimpl(new Impl(algorithm))
    {
    }
    HMAC_DRBG::~HMAC_DRBG() { pimpl->cleanup(); }

    void HMAC_DRBG::instantiate(uint32_t entropy_input_len, uint8_t *entropy_input,
                                uint32_t nonce_len, uint8_t *nonce,
                                uint32_t personalization_string_len,
                                uint8_t *personalization_string) const
    {
        Hacl_HMAC_DRBG_instantiate(pimpl->definition, pimpl->state, entropy_input_len,
                                   entropy_input, nonce_len, nonce, personalization_string_len,
                                   personalization_string);
    }

    bool HMAC_DRBG::generate(uint8_t *output, uint32_t n, uint32_t additional_input_len,
                             uint8_t *additional_input) const
    {
        return Hacl_HMAC_DRBG_generate(pimpl->definition, output, pimpl->state, n,
                                       additional_input_len, additional_input);
    }
} // namespace hacl