#ifndef __Hacl_LIB_HPP_INCLUDED__
#define __Hacl_LIB_HPP_INCLUDED__

#include <cstdint>

namespace hacl
{
    class Lib
    {
      public:
        static void memZero(void *dst, uint64_t len);
        static bool readRandomBytes(uint32_t len, uint8_t *buf, bool awaitEntropy = true);
    };
} // namespace hacl

#endif /* __Hacl_LIB_HPP_INCLUDED__ */