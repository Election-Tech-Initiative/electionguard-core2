#include "Hacl_HMAC.hpp"

#include "Hacl_HMAC.h"

using std::vector;

namespace hacl
{
    uint8_t get_hash_size_in_bytes(HMACAlgorithm algorithm)
    {
        switch (algorithm) {
            case HMACAlgorithm::SHA2_384:
                return 48U;
            case HMACAlgorithm::SHA2_512:
                return 64U;
            case HMACAlgorithm::BLAKE2b_32:
                return 64U;
            case HMACAlgorithm::BLAKE2s_32:
                return 32U;
            case HMACAlgorithm::LEGACY_SHA1:
                return 20U;
            case HMACAlgorithm::SHA2_256:
            default:
                return 32U;
        }
    }

    vector<uint8_t> HMAC::compute(const vector<uint8_t> &key, const vector<uint8_t> &data,
                                  HMACAlgorithm algorithm /*= HMACAlgorithm::SHA2_256 */)

    {
        auto hashSize = get_hash_size_in_bytes(algorithm);
        vector<uint8_t> hmac(hashSize, 0);

        HMAC::compute(&hmac.front(), const_cast<uint8_t *>(&key.front()), key.size(),
                      const_cast<uint8_t *>(&data.front()), data.size(), algorithm);

        return hmac;
    }

    void HMAC::compute(uint8_t *dst, uint8_t *key, uint32_t key_len, uint8_t *data,
                       uint32_t data_len, HMACAlgorithm algorithm /* = HMACAlgorithm::SHA2_256 */)
    {
        switch (algorithm) {
            case HMACAlgorithm::SHA2_384:
                Hacl_HMAC_compute_sha2_384(dst, key, key_len, data, data_len);
                break;
            case HMACAlgorithm::SHA2_512:
                Hacl_HMAC_compute_sha2_512(dst, key, key_len, data, data_len);
                break;
            case HMACAlgorithm::BLAKE2b_32:
                Hacl_HMAC_compute_blake2b_32(dst, key, key_len, data, data_len);
                break;
            case HMACAlgorithm::BLAKE2s_32:
                Hacl_HMAC_compute_blake2s_32(dst, key, key_len, data, data_len);
                break;
            case HMACAlgorithm::LEGACY_SHA1:
                Hacl_HMAC_legacy_compute_sha1(dst, key, key_len, data, data_len);
                break;
            case HMACAlgorithm::SHA2_256:
            default:
                Hacl_HMAC_compute_sha2_256(dst, key, key_len, data, data_len);
                break;
        }
    }

} // namespace hacl
