#ifndef __Hacl_HMAC_DRBG_HPP_INCLUDED__
#define __Hacl_HMAC_DRBG_HPP_INCLUDED__

#include <cstdint>
#include <memory>
#include <vector>

namespace hacl
{
    enum HMAC_DRBG_HashAlgorithm { SHA2_256 = 0, SHA2_384 = 1, SHA2_512 = 2, LEGACY_SHA1 = 999 };

    class HMAC_DRBG
    {
      public:
        explicit HMAC_DRBG(HMAC_DRBG_HashAlgorithm algorithm = HMAC_DRBG_HashAlgorithm::SHA2_256);
        ~HMAC_DRBG();

        void instantiate(uint32_t entropy_input_len, uint8_t *entropy_input, uint32_t nonce_len,
                         uint8_t *nonce, uint32_t personalization_string_len,
                         uint8_t *personalization_string) const;

        bool generate(uint8_t *output, uint32_t n, uint32_t additional_input_len,
                      uint8_t *additional_input) const;

      private:
        struct Impl;
        std::unique_ptr<Impl> pimpl;
    };
} // namespace hacl

#endif /* __Hacl_HMAC_DRBG_HPP_INCLUDED__ */