#include "Lib.hpp"

#include "Lib_Memzero0.h"
#include "Lib_RandomBuffer_System.h"

namespace hacl
{
    void Lib::memZero(void *dst, uint64_t len) { Lib_Memzero0_memzero(dst, len); }

    bool Lib::readRandomBytes(uint32_t len, uint8_t *buf, bool awaitEntropy /* = true */)
    {
        if (awaitEntropy) {
            Lib_RandomBuffer_System_crypto_random(buf, len);
            return true;
        }
        return Lib_RandomBuffer_System_randombytes(buf, len);
    }
} // namespace hacl