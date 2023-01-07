#ifndef __Hacl_HMAC_HPP_INCLUDED__
#define __Hacl_HMAC_HPP_INCLUDED__

#include <cstdint>
#include <vector>

namespace hacl
{
    enum HMACAlgorithm {
        SHA2_256 = 0,
        SHA2_384 = 1,
        SHA2_512 = 2,
        BLAKE2s_32 = 3,
        BLAKE2b_32 = 4,
        LEGACY_SHA1 = 999
    };

    class HMAC
    {
      public:
        static std::vector<uint8_t> compute(const std::vector<uint8_t> &key,
                                            const std::vector<uint8_t> &data,
                                            HMACAlgorithm algorithm = HMACAlgorithm::SHA2_256);
        static void compute(uint8_t *dst, uint8_t *key, uint32_t key_len, uint8_t *data,
                            uint32_t data_len, HMACAlgorithm algorithm = HMACAlgorithm::SHA2_256);
    };
} // namespace hacl

#endif /* __Hacl_HMAC_HPP_INCLUDED__ */